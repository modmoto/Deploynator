namespace DevLab.AzureAdapter.DTOs
{
    public static class AzureConstants
    {
        //TODO add cancel state 
        public const string ENVIRONMENT_STATUS_SUCCEEDED = "Succeeded";
        public const string ENVIRONMENT_STATUS_NOT_DEPLOYED = "NotDeployed";
        public const string ENVIRONMENT_STATUS_NOT_STARTED = "NotStarted";
        public const string ENVIRONMENT_STATUS_REJECTED = "Rejected";
        public const string ENVIRONMENT_STATUS_CANCELED = "Canceled";
        public const string OPERATION_STATUS_APPROVED = "Approved";
        public const string OPERATION_STATUS_PENDING = "Pending";
    }
}