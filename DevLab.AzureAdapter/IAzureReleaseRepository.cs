using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;

public interface IAzureReleaseRepository{
    Task<IEnumerable<ReleaseDefinition>> GetReleaseDefinitionsAsync();

    Task<DeploymentResult> DeployToProdAsync(int releaseDefinitionId);
}