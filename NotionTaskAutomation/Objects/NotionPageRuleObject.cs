using System;

namespace NotionTaskAutomation.Objects;

public class NotionPageRuleObject
{
    public string StartingState { get; set; }
    public string EndingState { get; set; }
    public string OnDay { get; set; }
    public int DayOffset { get; set; }
}
