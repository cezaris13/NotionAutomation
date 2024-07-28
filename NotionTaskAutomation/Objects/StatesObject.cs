using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionTaskAutomation.Objects;

public class StatesObject
{
    [JsonPropertyName("properties")]
    public Properties Properties { get; set; }
}

public class Properties
{
    [JsonPropertyName("Status")]
    public StatusObject Status { get; set; }
}

public class StatusObject
{
    [JsonPropertyName("select")]
    public SelectObject Select { get; set; }
}

public class SelectObject
{
    [JsonPropertyName("options")]
    public List<Options> Options { get; set; }
}

public class Options
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}