using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotionAutomation.Objects;

namespace NotionAutomation;

public interface INotionApiService {
    Task<List<TaskObject>> GetTasks(Guid notionDatabaseId);
    Task<List<string>> GetStates(Guid notionDatabaseId);
    Task UpdateTasks(Guid notionDatabaseId);
    List<NotionDatabaseRule> GetNotionDatabaseRules(Guid notionDatabaseId);
    Task<List<Guid>> GetSharedDatabases();
}