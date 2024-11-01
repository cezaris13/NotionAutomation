using System;
using System.Text.Json.Serialization;

namespace NotionTaskAutomation.Objects;

public class PropertyObject
{
    [JsonPropertyName("Status")]
    public Status Status { get; set; }

    [JsonPropertyName("Date")]
    public DateObject Date { get; set; }
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

public class DateObject
{
    [JsonPropertyName("date")]
    public DateTimeObject DateTimeObject { get; set; }
}

public class DateTimeObject
{
    [JsonPropertyName("start")]
    public DateTime Date { get; set; }
}