namespace DevLab.AzureAdapter.DTOs
{
    public class DeploymentResult
    {
        private DeploymentResult() { }
        private DeploymentResult(int releaseDefinitionId, string releaseName, int? releaseId, bool deployed = true)
        {
            ReleaseDefinitionId = releaseDefinitionId;
            ReleaseName = releaseName;
            ReleaseId = releaseId;
            Deployed = deployed;
        }

        public int ReleaseDefinitionId { get; private set; }
        public string ReleaseName { get; }
        public int? ReleaseId { get; private set; }
        public bool Deployed { get; private set; }

        public static DeploymentResult Success(int releaseDefinitionId, string releaseName, int releaseId)
        {
            return new DeploymentResult(releaseDefinitionId, releaseName, releaseId);
        }

        public static DeploymentResult Failed(int releaseDefinitionId, string releaseName)
        {
            return new DeploymentResult(releaseDefinitionId, releaseName, null, false);
        }
    }
}