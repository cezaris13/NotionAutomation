using System.Text.Json.Serialization;

namespace NotionAutomationButtonAutomation.Objects
{
    public class PropertiesObject
    {
        [JsonPropertyName("properties")] 
        public PropertyObject Properties { get; set; }
    }

    public class PropertyObject
    {
        [JsonPropertyName("Status")] 
        public Status Status { get; set; }
    }

    public class Status
    {
        [JsonPropertyName("select")] 
        public Select Select { get; set; }
    }

    public class Select
    { 
        [JsonPropertyName("name")] 
        public string Name { get; set; }
    }
}