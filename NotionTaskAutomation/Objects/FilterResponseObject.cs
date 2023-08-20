using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionTaskAutomation.Objects;

public class FilterResponseObject
{
    [JsonPropertyName("result")]
    public Result Result { get; set; }
}

public class Result
{
    [JsonPropertyName("reducerResults")]
    public ReducerResults ReducerResults { get; set; }
}

public class ReducerResults
{
    [JsonPropertyName("results")]
    public FilterResults Results { get; set; }
}

public class FilterResults
{
    [JsonPropertyName("blockIds")]
    public List<Guid> BlockIds { get; set; }
}