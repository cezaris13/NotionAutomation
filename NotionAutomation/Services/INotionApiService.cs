using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotionAutomation.DataTypes;

namespace NotionAutomation.Services;

public interface INotionApiService {
    Task<Result<List<string>, ActionResult>> GetStates(Guid notionDatabaseId);
    List<NotionDatabaseRule> GetNotionDatabaseRules(Guid notionDatabaseId);
    Task<Result<List<TaskObject>, ActionResult>> GetTasks(Guid notionDatabaseId);
    Task<Result<List<Guid>, ActionResult>> GetSharedDatabases();
    Task<Result<Unit, ActionResult>> UpdateTasks(Guid notionDatabaseId);
}