using System.Collections.Generic;
using System.Threading.Tasks;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public interface INotionButtonClicker
{
    Task<List<TaskObject>> GetTasks();
    Task<List<string>> GetStates();
    Task UpdateTasks();
}
