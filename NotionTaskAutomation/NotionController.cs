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
    [Route("getPageRules")]
    public async Task<ActionResult<List<NotionPageRule>>> GetNotionPageRules(Guid notionPageId)
    {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionPageIds.Contains(notionPageId))
            return NotFound("Notion page not found");

        return m_notionButtonClicker.GetNotionPageRules(notionPageId);
    }

    [HttpGet]
    [Route("getPageRule")]
    public async Task<ActionResult<NotionPageRule>> GetNotionPageRule(Guid notionPageRuleId)
    {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        var notionPageRule =
            await m_notionDbContext.NotionPageRules.FirstOrDefaultAsync(p => p.RuleId == notionPageRuleId);
        if (!notionPageIds.Contains(notionPageRule.PageId))
            return NotFound("Notion page not found");

        return notionPageRule;
    }

    [HttpPatch]
    [Route("modifyPageRule")]
    public async Task<ActionResult> ModifyNotionPageRule(Guid notionPageId,
        NotionPageRule notionPageRuleObject)
    {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionPageIds.Contains(notionPageId))
            return NotFound("Notion page not found");

        var states = await m_notionButtonClicker.GetStates(notionPageId);

        if (states.FindIndex(p => p == notionPageRuleObject.StartingState) == -1 ||
            states.FindIndex(p => p == notionPageRuleObject.EndingState) == -1)
            return BadRequest("Notion page start or end state is invalid");

        var notionPageRule =
            await m_notionDbContext.NotionPageRules.FirstOrDefaultAsync(p => p.RuleId == notionPageRuleObject.RuleId);

        if (notionPageRule == null)
            return NotFound("Notion page rule not found");

        notionPageRule.PageId = notionPageId;
        notionPageRule.DayOffset = notionPageRuleObject.DayOffset;
        notionPageRule.StartingState = notionPageRuleObject.StartingState;
        notionPageRule.EndingState = notionPageRuleObject.EndingState;
        notionPageRule.OnDay = notionPageRuleObject.OnDay;

        m_notionDbContext.NotionPageRules.Update(notionPageRule);
        await m_notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Route("addPageRule")]
    public async Task<ActionResult> AddNotionPageRule(Guid notionPageId, NotionPageRuleObject notionPageRuleObject)
    {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionPageIds.Contains(notionPageId))
            return NotFound("Notion page not found");

        var states = await m_notionButtonClicker.GetStates(notionPageId);

        if (states.FindIndex(p => p == notionPageRuleObject.StartingState) == -1 ||
            states.FindIndex(p => p == notionPageRuleObject.EndingState) == -1)
            return BadRequest("Notion page start or end state is invalid");

        var notionPageRule = new NotionPageRule
        {
            RuleId = Guid.NewGuid(),
            PageId = notionPageId,
            DayOffset = notionPageRuleObject.DayOffset,
            OnDay = notionPageRuleObject.OnDay,
            StartingState = notionPageRuleObject.StartingState,
            EndingState = notionPageRuleObject.EndingState
        };

        await m_notionDbContext.NotionPageRules.AddAsync(notionPageRule);
        await m_notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Route("removePageRule")]
    public async Task<ActionResult> DeleteNotionPageRule(Guid notionPageRuleId)
    {
        var notionPageRule = await m_notionDbContext.NotionPageRules.FirstAsync(p => p.RuleId == notionPageRuleId);
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if (!notionPageIds.Contains(notionPageRule.PageId))
            return NotFound("Notion page not found");

        m_notionDbContext.NotionPageRules.Remove(notionPageRule);
        await m_notionDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("getStates")]
    public async Task<ActionResult<List<string>>> GetStates(Guid notionPageId) {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if(!notionPageIds.Contains(notionPageId))
            return NotFound("Notion page not found");

        return await m_notionButtonClicker.GetStates(notionPageId);
    }

    [HttpGet]
    [Route("getTasks")]
    public async Task<ActionResult<List<TaskObject>>> GetTasks(Guid notionPageId)
    {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if(!notionPageIds.Contains(notionPageId))
            return NotFound("Notion page not found");

        return await m_notionButtonClicker.GetTasks(notionPageId);
    }

    [HttpGet]
    [Route("updateNotionPage")]
    public async Task<ActionResult> UpdateTasksForDatabase(Guid notionPageId)
    {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        if(!notionPageIds.Contains(notionPageId))
            return NotFound("Notion page not found");

        await m_notionButtonClicker.UpdateTasks(notionPageId);

        return Ok();
    }

    [HttpGet]
    [Route("updateNotionPages")]
    public async Task<ActionResult> UpdateTasksForDatabases() {
        var notionPageIds = await m_notionButtonClicker.GetSharedDatabases();

        foreach (var notionPageId in notionPageIds)
            await m_notionButtonClicker.UpdateTasks(notionPageId);

        return Ok();
    }
}