using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionAutomation.Objects;

public class TasksFilter {
    [JsonPropertyName("start_cursor")] public Guid? StartCursor { get; set; }

    [JsonPropertyName("filter")] public Filter Filter { get; set; }
}

public class Filter {
    [JsonPropertyName("or")] public List<Or> Or { get; set; }
}

public class Or {
    [JsonPropertyName("and")] public List<And> And { get; set; }
}

public class And {
    [JsonPropertyName("property")] public string Property { get; set; }

    [JsonPropertyName("select")] public FilterSelect Select { get; set; }

    [JsonPropertyName("date")] public FilterDateObject Date { get; set; }
}

public class FilterSelect {
    [JsonPropertyName("equals")] public new string Equals { get; set; }
}

public class FilterDateObject {
    [JsonPropertyName("on_or_before")] public string OnOrBefore { get; set; }

    [JsonPropertyName("before")] public string Before { get; set; }
}