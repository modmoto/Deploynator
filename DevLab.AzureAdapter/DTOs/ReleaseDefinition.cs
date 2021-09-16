using System.Collections.Generic;

namespace DevLab.AzureAdapter.DTOs
{
    public class ReleaseDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ReleaseDefinitionList
    {
        public int Count { get; set; }
        public IEnumerable<ReleaseDefinition> Value { get; set; }
    }
}
