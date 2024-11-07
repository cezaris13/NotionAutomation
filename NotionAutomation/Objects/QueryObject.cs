using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionAutomation.Objects;

public class QueryObject
{
    [JsonPropertyName("results")]
    public List<TaskObject> Results { get; set; }

    [JsonPropertyName("next_cursor")]
    public Guid? NextCursor { get; set; }
}

public class TaskObject
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("properties")]
    public PropertyObject Properties { get; set; }
}

public class UpdateTaskObject
{
    [JsonPropertyName("properties")]
    public PropertyObject Properties { get; set; }
}