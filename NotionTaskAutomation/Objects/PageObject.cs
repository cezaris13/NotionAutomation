using System;
using System.Text.Json.Serialization;

namespace NotionTaskAutomation.Objects;

public class PageObject
{
    [JsonPropertyName("pageId")]
    public Guid PageId { get; set; }

    [JsonPropertyName("spaceId")]
    public Guid SpaceId { get; set; }
}