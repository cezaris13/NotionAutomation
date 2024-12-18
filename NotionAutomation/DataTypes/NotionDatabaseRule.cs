using System;
using System.ComponentModel.DataAnnotations;

namespace NotionAutomation.DataTypes;

public class NotionDatabaseRule {
    [Key] public Guid RuleId { get; set; }

    public Guid DatabaseId { get; set; }
    public string StartingState { get; set; }
    public string EndingState { get; set; }
    public DateCondition DateCondition { get; set; }
    public int DayOffset { get; set; }
}