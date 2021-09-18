using System.Collections.Generic;
using System.Threading.Tasks;
using DevLab.AzureAdapter.DTOs;

namespace DevLab.AzureAdapter
{
    public class FakeAzureReleaseRepository : IAzureReleaseRepository
    {
        private List<ReleaseDefinition> _releaseDefinitions = new()
        {
            new()
            {
                Id = 1,
                Name = "God Service Fake"
            },
            new()
            {
                Id = 2,
                Name = "DB Zentrale Fake"
            },
            new()
            {
                Id = 3,
                Name = "Moritz Service Fake"
            },
        };

        public Task<List<ReleaseDefinition>> GetReleaseDefinitionsAsync()
        {
            return Task.FromResult(_releaseDefinitions);
        }

        public async Task<IEnumerable<DeploymentResult>> DeployReleasesToProdAsync(
            IEnumerable<ReleaseDefinition> releaseDefinitions)
        {
            await Task.Delay(20000);
            var definitions = new List<DeploymentResult>();
            foreach (var r in releaseDefinitions)
            {
                definitions.Add(r.Id % 2 == 0
                    ? DeploymentResult.Failed(r.Id, r.Name)
                    : DeploymentResult.Success(r.Id, r.Name, r.Id));
            }

            return definitions;
        }
    }
}