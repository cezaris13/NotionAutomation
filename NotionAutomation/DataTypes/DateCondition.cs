using System.Text.Json.Serialization;

namespace NotionAutomation.DataTypes;

[JsonConverter(typeof(JsonStringEnumConverter<DateCondition>))]
public enum DateCondition {
    OnOrBefore,
    Before,
}