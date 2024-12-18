using System;
using System.Text.Json.Serialization;

namespace NotionAutomation.DataTypes;

public class PropertyObject {
    [JsonPropertyName("Status")] public Status Status { get; set; }
    
    [JsonPropertyName("Date")] public NotionDate? Date { get; set; }
}

public class Status {
    [JsonPropertyName("select")] public Select Select { get; set; }
}

public class Select {
    [JsonPropertyName("name")] public string Name { get; set; }
}

public class NotionDate {
    [JsonPropertyName("date")] public DateObject? DateObject { get; set; }
}

public class DateObject {
    [JsonPropertyName("start")] public DateTime? StartDate { get; set; }
    
    [JsonPropertyName("end")] public DateTime? EndDate { get; set; }
}
