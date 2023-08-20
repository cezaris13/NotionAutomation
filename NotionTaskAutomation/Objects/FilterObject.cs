using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionTaskAutomation.Objects;

public class FilterObject
{
    [JsonPropertyName("source")]
    public Source Source { get; set; }

    [JsonPropertyName("collectionView")]
    public CollectionView CollectionView { get; set; }

    [JsonPropertyName("loader")]
    public Loader Loader { get; set; }
}

public class Source
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("spaceId")]
    public Guid SpaceId { get; set; }
}

public class CollectionView
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("spaceId")]
    public Guid SpaceId { get; set; }
}

public class Loader
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("filter")]
    public FiltersList Filter { get; set; }

    [JsonPropertyName("userTimeZone")]
    public string UserTimeZone { get; set; }

    [JsonPropertyName("reducers")]
    public Reducers Reducers { get; set; }
}

public class Reducers
{
    [JsonPropertyName("results")]
    public Results Results { get; set; }

    [JsonPropertyName("total")]
    public Total Total { get; set; }
}

public class Results
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

public class Total
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("aggregation")]
    public Aggregation Aggregation { get; set; }
}

public class Aggregation
{
    [JsonPropertyName("aggregator")]
    public string Aggregator { get; set; }
}

public class FiltersList
{
    [JsonPropertyName("Filters")]
    public List<FilterObjectWithProperty> Filters { get; set; }

    [JsonPropertyName("operator")]
    public string Operator { get; set; }

    [JsonPropertyName("filters")]
    public List<FiltersList> FiltersRec { get; set; }
}

public class FilterObjectWithProperty
{
    [JsonPropertyName("filter")]
    public OneFilterObject FilterObject { get; set; }

    [JsonPropertyName("property")]
    public string Property { get; set; }
}

public class OneFilterObject
{
    [JsonPropertyName("value")]
    public Value Value { get; set; }

    [JsonPropertyName("operator")]
    public string Operator { get; set; }
}

public class Value
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public string ValueString { get; set; }
}