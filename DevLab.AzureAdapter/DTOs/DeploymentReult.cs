namespace DevLab.AzureAdapter.DTOs
{
    public class DeploymentResult
    {
        public static DeploymentResult Failed => new DeploymentResult();

        private DeploymentResult() { }
        private DeploymentResult(int releaseId)
        {
            ReleaseId = releaseId;
            Deployed = true;
        }

        public int? ReleaseId { get; private set; }
        public bool Deployed { get; private set; }

        public static DeploymentResult Success(int releaseId)
        {
            return new DeploymentResult(releaseId);
        }
    }
}