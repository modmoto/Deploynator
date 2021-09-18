using System.Collections.Generic;
using System.Threading.Tasks;
using DevLab.AzureAdapter.DTOs;

namespace DevLab.AzureAdapter
{
    public interface IAzureReleaseRepository{
        Task<List<ReleaseDefinition>> GetReleaseDefinitionsAsync();
        IEnumerable<DeploymentResult> DeployReleasesToProdAsync(IEnumerable<ReleaseDefinition> releaseDefinitions);
    }
}