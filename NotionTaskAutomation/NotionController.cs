using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotionTaskAutomation.Db;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

[ApiController]
[Route("api")]
public class NotionController : Controller
{
    private readonly INotionButtonClicker m_notionButtonClicker;
    private readonly NotionDbContext m_notionDbContext;

    public NotionController(INotionButtonClicker notionButtonClicker, NotionDbContext notionDbContext)
    {
        m_notionButtonClicker = notionButtonClicker;
        m_notionDbContext = notionDbContext;
    }

    [HttpGet]
    [Route("getSharedDatabases")]
    public async Task<ActionResult<List<Guid>>> GetSharedDatabases()
    {
        return await m_notionButtonClicker.GetSharedDatabases();
    }

    [HttpGet]
    [Route("getDatabaseRules")]
    public async Task<ActionResult<List<NotionDatabaseRule>>> GetNotionDatabaseRules(Guid notionDatabaseId)
    {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionDatabaseIds.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        return m_notionButtonClicker.GetNotionDatabaseRules(notionDatabaseId);
    }

    [HttpGet]
    [Route("getDatabaseRule")]
    public async Task<ActionResult<NotionDatabaseRule>> GetNotionDatabaseRule(Guid notionDatabaseRuleId)
    {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        var notionDatabaseRule =
            await m_notionDbContext.NotionDatabaseRules.FirstOrDefaultAsync(p => p.RuleId == notionDatabaseRuleId);
        if (!notionDatabaseIds.Contains(notionDatabaseRule.DatabaseId))
            return NotFound("Notion database not found");

        return notionDatabaseRule;
    }

    [HttpPatch]
    [Route("modifyDatabaseRule")]
    public async Task<ActionResult> ModifyNotionDatabaseRule(Guid notionDatabaseId,
        NotionDatabaseRule notionDatabaseRuleObject)
    {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionDatabaseIds.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var states = await m_notionButtonClicker.GetStates(notionDatabaseId);

        if (states.FindIndex(p => p == notionDatabaseRuleObject.StartingState) == -1 ||
            states.FindIndex(p => p == notionDatabaseRuleObject.EndingState) == -1)
            return BadRequest("Notion page start or end state is invalid");

        var notionDatabaseRule =
            await m_notionDbContext.NotionDatabaseRules.FirstOrDefaultAsync(p => p.RuleId == notionDatabaseRuleObject.RuleId);

        if (notionDatabaseRule == null)
            return NotFound("Notion page rule not found");

        notionDatabaseRule.DatabaseId = notionDatabaseId;
        notionDatabaseRule.DayOffset = notionDatabaseRuleObject.DayOffset;
        notionDatabaseRule.StartingState = notionDatabaseRuleObject.StartingState;
        notionDatabaseRule.EndingState = notionDatabaseRuleObject.EndingState;
        notionDatabaseRule.OnDay = notionDatabaseRuleObject.OnDay;

        m_notionDbContext.NotionDatabaseRules.Update(notionDatabaseRule);
        await m_notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Route("addDatabaseRule")]
    public async Task<ActionResult> AddNotionDatabaseRule(Guid notionDatabaseId, NotionDatabaseRuleObject notionDatabaseRuleObject)
    {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionDatabaseIds.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        var states = await m_notionButtonClicker.GetStates(notionDatabaseId);

        if (states.FindIndex(p => p == notionDatabaseRuleObject.StartingState) == -1 ||
            states.FindIndex(p => p == notionDatabaseRuleObject.EndingState) == -1)
            return BadRequest("Notion page start or end state is invalid");

        var notionDatabaseRule = new NotionDatabaseRule
        {
            RuleId = Guid.NewGuid(),
            DatabaseId = notionDatabaseId,
            DayOffset = notionDatabaseRuleObject.DayOffset,
            OnDay = notionDatabaseRuleObject.OnDay,
            StartingState = notionDatabaseRuleObject.StartingState,
            EndingState = notionDatabaseRuleObject.EndingState
        };

        await m_notionDbContext.NotionDatabaseRules.AddAsync(notionDatabaseRule);
        await m_notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Route("removeDatabaseRule")]
    public async Task<ActionResult> DeleteNotionDatabaseRule(Guid notionDatabaseRuleId)
    {
        var notionDatabaseRule = await m_notionDbContext.NotionDatabaseRules.FirstAsync(p => p.RuleId == notionDatabaseRuleId);
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionDatabaseIds.Contains(notionDatabaseRule.DatabaseId))
            return NotFound("Notion database not found");

        m_notionDbContext.NotionDatabaseRules.Remove(notionDatabaseRule);
        await m_notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("getStates")]
    public async Task<ActionResult<List<string>>> GetStates(Guid notionDatabaseId) {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if(!notionDatabaseIds.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        return await m_notionButtonClicker.GetStates(notionDatabaseId);
    }

    [HttpGet]
    [Route("getTasks")]
    public async Task<ActionResult<List<TaskObject>>> GetTasks(Guid notionDatabaseId)
    {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if(!notionDatabaseIds.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        return await m_notionButtonClicker.GetTasks(notionDatabaseId);
    }

    [HttpGet]
    [Route("updateNotionDatabase")]
    public async Task<ActionResult> UpdateTasksForDatabase(Guid notionDatabaseId)
    {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        if(!notionDatabaseIds.Contains(notionDatabaseId))
            return NotFound("Notion database not found");

        await m_notionButtonClicker.UpdateTasks(notionDatabaseId);

        return Ok();
    }

    [HttpGet]
    [Route("updateNotionDatabases")]
    public async Task<ActionResult> UpdateTasksForDatabases() {
        var notionDatabaseIds = await m_notionButtonClicker.GetSharedDatabases();

        foreach (var notionDatabaseId in notionDatabaseIds)
            await m_notionButtonClicker.UpdateTasks(notionDatabaseId);

        return Ok();
    }
}