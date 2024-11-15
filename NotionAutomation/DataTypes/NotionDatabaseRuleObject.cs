namespace NotionAutomation.DataTypes;

public class NotionDatabaseRuleObject {
    public string StartingState { get; set; }
    public string EndingState { get; set; }
    public DateCondition DateCondition { get; set; }
    public int DayOffset { get; set; }
}