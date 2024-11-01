using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public interface INotionButtonClicker
{
    Task<List<TaskObject>> GetTasks(Guid notionDatabaseId);
    Task<List<string>> GetStates(Guid notionDatabaseId);
    Task UpdateTasks(Guid notionDatabaseId);
    List<NotionDatabaseRule> GetNotionDatabaseRules(Guid notionDatabaseId);
    Task<List<Guid>> GetSharedDatabases();
}
