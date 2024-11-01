using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public interface INotionButtonClicker
{
    Task<List<TaskObject>> GetTasks(Guid notionPageId);
    Task<List<string>> GetStates(Guid notionPageId);
    Task UpdateTasks(Guid notionPageId);
    List<NotionPageRule> GetNotionPageRules(Guid notionPageId);
    Task<List<Guid>> GetSharedDatabases();
}
