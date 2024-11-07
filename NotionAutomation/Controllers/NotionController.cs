using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotionAutomation.Attributes;
using NotionAutomation.Db;
using NotionAutomation.Objects;

namespace NotionAutomation;

[ApiController]
[Route("api")]
public class NotionController(INotionApiService notionApiService, NotionDbContext notionDbContext)
    : Controller {
    [HttpGet]
    [Authorization]
    [Route("getSharedDatabases")]
    public async Task<ActionResult<List<Guid>>> GetSharedDatabases() {
        var sharedDatabases = await notionApiService.GetSharedDatabases();

        return sharedDatabases.Match<ActionResult<List<Guid>>>(
            sharedDatabases => Ok(sharedDatabases),
            err => err);
    }

    [HttpGet]
    [Authorization]
    [Route("getDatabaseRules")]
    public async Task<ActionResult<List<NotionDatabaseRule>>> GetNotionDatabaseRules(Guid notionDatabaseId) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        if (!notionDatabaseIds.Value.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        return Ok(notionApiService.GetNotionDatabaseRules(notionDatabaseId));
    }

    [HttpGet]
    [Authorization]
    [Route("getDatabaseRule")]
    public async Task<ActionResult<NotionDatabaseRule>> GetNotionDatabaseRule(Guid notionDatabaseRuleId) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        var notionDatabaseRule =
            await notionDbContext.NotionDatabaseRules.FirstOrDefaultAsync(p => p.RuleId == notionDatabaseRuleId);

        if (notionDatabaseRule == null)
            return NotFound("Notion page rule not found");

        if (!notionDatabaseIds.Value.Contains(notionDatabaseRule.DatabaseId))
            return NotFound("Notion database not found");

        return Ok(notionDatabaseRule);
    }

    [HttpPatch]
    [Authorization]
    [Route("modifyDatabaseRule")]
    public async Task<ActionResult> ModifyNotionDatabaseRule(Guid notionDatabaseId,
        NotionDatabaseRule notionDatabaseRuleObject) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        if (!notionDatabaseIds.Value.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var states = await notionApiService.GetStates(notionDatabaseId);
        if (!states.IsOk)
            return states.Error;

        if (states.Value.FindIndex(p => p == notionDatabaseRuleObject.StartingState) == -1 ||
            states.Value.FindIndex(p => p == notionDatabaseRuleObject.EndingState) == -1)
            return BadRequest("Notion page start or end state is invalid");

        var notionDatabaseRule =
            await notionDbContext.NotionDatabaseRules.FirstOrDefaultAsync(p =>
                p.RuleId == notionDatabaseRuleObject.RuleId);

        if (notionDatabaseRule == null)
            return NotFound("Notion page rule not found");

        notionDatabaseRule.DatabaseId = notionDatabaseId;
        notionDatabaseRule.DayOffset = notionDatabaseRuleObject.DayOffset;
        notionDatabaseRule.StartingState = notionDatabaseRuleObject.StartingState;
        notionDatabaseRule.EndingState = notionDatabaseRuleObject.EndingState;
        notionDatabaseRule.OnDay = notionDatabaseRuleObject.OnDay;

        notionDbContext.NotionDatabaseRules.Update(notionDatabaseRule);
        await notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Authorization]
    [Route("addDatabaseRule")]
    public async Task<ActionResult> AddNotionDatabaseRule(Guid notionDatabaseId,
        NotionDatabaseRuleObject notionDatabaseRuleObject) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        if (!notionDatabaseIds.Value.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var states = await notionApiService.GetStates(notionDatabaseId);

        if (!states.IsOk)
            return states.Error;

        if (states.Value.FindIndex(p => p == notionDatabaseRuleObject.StartingState) == -1 ||
            states.Value.FindIndex(p => p == notionDatabaseRuleObject.EndingState) == -1)
            return BadRequest("Notion page start or end state is invalid");

        var notionDatabaseRule = new NotionDatabaseRule {
            RuleId = Guid.NewGuid(),
            DatabaseId = notionDatabaseId,
            DayOffset = notionDatabaseRuleObject.DayOffset,
            OnDay = notionDatabaseRuleObject.OnDay,
            StartingState = notionDatabaseRuleObject.StartingState,
            EndingState = notionDatabaseRuleObject.EndingState
        };

        await notionDbContext.NotionDatabaseRules.AddAsync(notionDatabaseRule);
        await notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Authorization]
    [Route("removeDatabaseRule")]
    public async Task<ActionResult> DeleteNotionDatabaseRule(Guid notionDatabaseRuleId) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();
        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        var notionDatabaseRule =
            await notionDbContext.NotionDatabaseRules.FirstOrDefaultAsync(p => p.RuleId == notionDatabaseRuleId);

        if (notionDatabaseRule == null)
            return NotFound("Notion rule for database not found");

        if (!notionDatabaseIds.Value.Contains(notionDatabaseRule.DatabaseId))
            return NotFound("Notion database not found");

        notionDbContext.NotionDatabaseRules.Remove(notionDatabaseRule);
        await notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Authorization]
    [Route("getStates")]
    public async Task<ActionResult<List<string>>> GetStates(Guid notionDatabaseId) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        if (!notionDatabaseIds.Value.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var states = await notionApiService.GetStates(notionDatabaseId);

        return states.Match(
            states => Ok(states),
            errors => errors
        );
    }

    [HttpGet]
    [Authorization]
    [Route("getTasks")]
    public async Task<ActionResult<List<TaskObject>>> GetTasks(Guid notionDatabaseId) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        if (!notionDatabaseIds.Value.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var taskResult = await notionApiService.GetTasks(notionDatabaseId);

        return taskResult.Match(
            tasks => Ok(notionDatabaseId),
            error => error
        );
    }

    [HttpGet]
    [Authorization]
    [Route("updateNotionDatabase")]
    public async Task<ActionResult> UpdateTasksForDatabase(Guid notionDatabaseId) {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        if (!notionDatabaseIds.Value.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var response = await notionApiService.UpdateTasks(notionDatabaseId);

        return !response.IsOk ? response.Error : Ok();
    }

    [HttpGet]
    [Authorization]
    [Route("updateNotionDatabases")]
    public async Task<ActionResult> UpdateTasksForDatabases() {
        var notionDatabaseIds = await notionApiService.GetSharedDatabases();

        if (!notionDatabaseIds.IsOk)
            return notionDatabaseIds.Error;

        foreach (var notionDatabaseId in notionDatabaseIds.Value) {
            var response = await notionApiService.UpdateTasks(notionDatabaseId);
            if (!response.IsOk)
                return response.Error;
        }

        return Ok();
    }
}