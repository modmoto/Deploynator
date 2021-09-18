using System.Collections.Generic;
using System.Threading.Tasks;
using DevLab.AzureAdapter.DTOs;

namespace DevLab.AzureAdapter
{
    class FakeAzureReleaseRepository : IAzureReleaseRepository
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
            await Task.Delay(10000);
            return new List<DeploymentResult>
            {
                DeploymentResult.Success(_releaseDefinitions[0].Id, _releaseDefinitions[0].Name, _releaseDefinitions[0].Id),
                DeploymentResult.Failed(_releaseDefinitions[1].Id, _releaseDefinitions[1].Name),
                DeploymentResult.Success(_releaseDefinitions[2].Id, _releaseDefinitions[2].Name, _releaseDefinitions[0].Id)
            };
        }
    }
}