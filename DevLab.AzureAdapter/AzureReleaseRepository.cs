using System;
using System.Collections.Concurrent;
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



        public IEnumerable<DeploymentResult> DeployReleasesToProdAsync(IEnumerable<ReleaseDefinition> releaseDefinitions)
        {
            var tasks = releaseDefinitions.Select(DeployReleaseToProdWithStatusResult).ToArray();
            Task.WaitAll(tasks);

            return tasks.Select(x => x.Result).ToList();
        }

        public async Task<DeploymentResult> DeployReleaseToProdWithStatusResult(ReleaseDefinition releaseDefinition)
        {
            var releaseResult = await DeployReleaseToProdAsync(releaseDefinition);

            if (!releaseResult.Deployed) 
                return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);

            while (true)
            {
                var releaseStatusResponse = await _httpClient.GetAsync($"release/releases/{releaseResult.ReleaseId}?api-version=6.0");
                if (!releaseStatusResponse.IsSuccessStatusCode)
                {
                    return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);
                }

                var releaseStatusResponseContent = await releaseStatusResponse.Content.ReadAsStringAsync();
                var releaseStatus = JsonSerializer.Deserialize<ReleaseInformation>(releaseStatusResponseContent, _jsonOptions);

                if (releaseStatus.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_REJECTED,
                        StringComparison.InvariantCultureIgnoreCase)
                    || releaseStatus.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_CANCELED,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);
                }

                if (releaseStatus.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return DeploymentResult.Success(releaseDefinition.Id, releaseDefinition.Name, releaseStatus.Id);
                }

                await Task.Delay(3000);
            }
        }

        private async Task<DeploymentResult> DeployReleaseToProdAsync(ReleaseDefinition releaseDefinition)
        {
            var allReleases = await GetAllReleasesForDefintionIdAsync(releaseDefinition.Id);
            if (allReleases == null || allReleases.Value == null || !allReleases.Value.Any())
                return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);

            var prodRelease = IdentifyPotentialProdRelease(allReleases);
            if (prodRelease == null)
                return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);

            if (prodRelease.Environments.Last().Status.Equals(AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!await DeployStageAsync(prodRelease))
                    return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);

                await Task.Delay(2500);
            }

            if (!await ApproveReleaseAsync(prodRelease.Id))
                return DeploymentResult.Failed(releaseDefinition.Id, releaseDefinition.Name);

            return DeploymentResult.Success(releaseDefinition.Id, releaseDefinition.Name, prodRelease.Id);
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
            var requestUri = $"release/definitions?api-version=6.0";
            var result = await _httpClient.GetAsync(requestUri);
            result.EnsureSuccessStatusCode();
            var jsonContent = await result.Content.ReadAsStringAsync();
            var releaseDefinitions = JsonSerializer.Deserialize<ReleaseDefinitionList>(jsonContent, _jsonOptions).Value;

            var validReleases = new List<ReleaseDefinition>();
            foreach (var releaseDefinition in releaseDefinitions)
            {
                var allReleases = await GetAllReleasesForDefintionIdAsync(releaseDefinition.Id);
                if (allReleases == null || allReleases.Value == null || !allReleases.Value.Any())
                    continue;

                var prodRelease = IdentifyPotentialProdRelease(allReleases);
                if (prodRelease == null)
                    continue;

                validReleases.Add(releaseDefinition);
            }

            return validReleases;
        }
    }
}
