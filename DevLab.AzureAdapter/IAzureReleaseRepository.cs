using System.Collections.Generic;
using System.Threading.Tasks;
using DevLab.AzureAdapter.DTOs;

namespace DevLab.AzureAdapter
{
    public interface IAzureReleaseRepository{
        Task<IEnumerable<ReleaseDefinition>> GetReleaseDefinitionsAsync();
        IEnumerable<DeploymentResult> DeployReleasesToProdAsync(IEnumerable<ReleaseDefinition> releaseDefinitions);
    }
}