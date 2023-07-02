using System.ComponentModel;

namespace NotionAutomationButtonAutomation
{
  
    public enum States
    {
        [Description("TODO tomorrow")]
        TodoTomorrow,
        [Description("To Do")]
        Todo,
        [Description("Doing")]
        Doing,
        [Description("Event")] 
        Event,
        [Description("Event done")]
        EventDone
    }

}