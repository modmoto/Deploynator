using System.Collections.Generic;

namespace DTOs
{
    public class ReleaseInformationList
    {
        public int Count { get; set; }
        public List<ReleaseInformation> Value { get; set; }
    }

    public class ReleaseInformation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Environment> Environments { get; set; }
    }

    public class Environment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public IEnumerable<DeployStep> DeploySteps { get; set; }
    }

    public class DeployStep
    {
        public string Reason { get; set; }
        public string Status { get; set; }
        public string OperationStatus { get; set; }
    }

    public class ApprovalList {
        public int Count {get; set;}
        public IEnumerable<Approval> Value {get; set;}
    }

    public class Approval {
        public int Id{get; set;}
    }

}
