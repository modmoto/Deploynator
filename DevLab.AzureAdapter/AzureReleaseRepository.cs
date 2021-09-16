using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DevLab.AzureAdapter.DTOs;

namespace DevLab.AzureAdapter
{
    public class AzureReleaseRepository : IAzureReleaseRepository
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AzureReleaseRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.PropertyNameCaseInsensitive = true;
            _jsonOptions.PropertyNamingPolicy = null;
            _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }
        public async Task<DeploymentResult> DeployToProdAsync(int releaseDefinitionId)
        {
            var allReleases = await GetAllReleasesForDefintionIdAsync(releaseDefinitionId);
            if (allReleases == null || allReleases.Value == null || !allReleases.Value.Any())
                return DeploymentResult.Failed;

            var prodRelease = IdentifyPotentialProdRelease(allReleases);
            if (prodRelease == null)
                return DeploymentResult.Failed;

            if (prodRelease.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!await DeployStageAsync(prodRelease))
                    return DeploymentResult.Failed;

                await Task.Delay(2500);
            }

            if (!await ApproveReleaseAsync(prodRelease.Id))
                return DeploymentResult.Failed;

            return DeploymentResult.Success(prodRelease.Id);
        }

        private async Task<bool> DeployStageAsync(ReleaseInformation releaseInformation)
        {
            var patchPayload = new
            {
                status = "inProgress"
            };
            var patchPayloadAsJson = JsonSerializer.Serialize(patchPayload);
            var patchContent = new StringContent(patchPayloadAsJson, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"release/releases/{releaseInformation.Id}/environments/{releaseInformation.Environments.Last().Id}?api-version=6.0-preview.6", patchContent);

            return response.IsSuccessStatusCode;
        }
        private async Task<bool> ApproveReleaseAsync(int prodReleaseId)
        {
            var availableApprovalsResponse = await _httpClient.GetAsync($"release/approvals?releaseIdsFilter={prodReleaseId}&api-version=6.0");
            if (!availableApprovalsResponse.IsSuccessStatusCode)
                return false;
            var availableApprovalsContent = await availableApprovalsResponse.Content.ReadAsStringAsync();
            var approval = JsonSerializer.Deserialize<ApprovalList>(availableApprovalsContent, _jsonOptions).Value.FirstOrDefault();
            if (approval == null)
                return false;
            var approvePayload = new
            {
                status = "approved",
                comments = "Deploynator certified"
            };

            var approvedPayloadAsJson = JsonSerializer.Serialize(approvePayload);
            var approvedContent = new StringContent(approvedPayloadAsJson, System.Text.Encoding.UTF8, "application/json");
            var approvedResponse = await _httpClient.PatchAsync($"release/approvals/{approval.Id}?api-version=6.0", approvedContent);

            return approvedResponse.IsSuccessStatusCode;
        }

        private async Task<ReleaseInformationList> GetAllReleasesForDefintionIdAsync(int releaseDefinitionId)
        {
            var allReleasesResponse = await _httpClient.GetAsync($"release/releases?definitionId={releaseDefinitionId}&$expand=environments&api-version=6.0");
            if (!allReleasesResponse.IsSuccessStatusCode)
                return null;

            var allReleasesContent = await allReleasesResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ReleaseInformationList>(allReleasesContent, _jsonOptions);
        }

        private ReleaseInformation IdentifyPotentialProdRelease(ReleaseInformationList availableReleases)
        {
            var potentialProdReleases = availableReleases.Value.Where(release => release.Environments != null && release.Environments.Any()).ToList();
            foreach (var release in potentialProdReleases)
            {
                if (release.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED, StringComparison.CurrentCultureIgnoreCase))
                    return null;

                if (release.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_REJECTED, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if ((release.Environments.Count() < 2 && !release.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED, StringComparison.CurrentCultureIgnoreCase))
                        //if more than two stages, take one with successfull at/uat stage
                        || (release.Environments.ElementAt(release.Environments.Count() - 2).Status.Equals(AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED, StringComparison.CurrentCultureIgnoreCase)
                        && !release.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED, StringComparison.CurrentCultureIgnoreCase)))
                    return release;
            }

            return null;
        }

        public async Task<IEnumerable<ReleaseDefinition>> GetReleaseDefinitionsAsync()
        {
            var result = await _httpClient.GetAsync($"release/definitions?api-version=6.0");
            result.EnsureSuccessStatusCode();

            Console.WriteLine(result.StatusCode);

            var jsonContent = await result.Content.ReadAsStringAsync();

            Console.WriteLine(jsonContent);
            return JsonSerializer.Deserialize<ReleaseDefinitionList>(jsonContent).Value;
        }
    }
}
