using System.Text.Json.Serialization;

namespace NotionAutomation.Objects;

public class SearchFilter
{
    [JsonPropertyName("filter")]
    public SearchFilterObject Filter { get; set; }
}

public class SearchFilterObject
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("property")]
    public string Property { get; set; }
}