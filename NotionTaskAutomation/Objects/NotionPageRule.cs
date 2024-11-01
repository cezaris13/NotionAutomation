using System;
using System.ComponentModel.DataAnnotations;

namespace NotionTaskAutomation.Objects;

public class NotionPageRule
{
    [Key]
    public Guid RuleId { get; set; }
    public Guid PageId { get; set; }
    public string StartingState { get; set; }
    public string EndingState { get; set; }
    public string OnDay { get; set; }
    public int DayOffset { get; set; }
}