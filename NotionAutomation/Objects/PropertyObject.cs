using System;
using System.Text.Json.Serialization;

namespace NotionAutomation.Objects;

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