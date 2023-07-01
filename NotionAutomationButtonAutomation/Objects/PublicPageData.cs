using System;
using System.Text.Json.Serialization;

namespace NotionAutomationButtonAutomation.Objects
{
    public class PublicPageData
    {
        [JsonPropertyName("type")] 
        public string Type { get; set; }
        [JsonPropertyName("name")] 
        public string Name { get; set; }
        [JsonPropertyName("blockId")] 
        public Guid BlockId { get; set; }
        [JsonPropertyName("shouldDuplicate")]
        public bool ShouldDuplicate { get; set; }
        [JsonPropertyName("requestedOnPublicDomain")]
        public bool RequestedOnPublicdomain { get; set; }
       
    }
}