using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

[ApiController]
public class NotionController : Controller
{
    private readonly INotionButtonClicker m_notionButtonClicker;
    
    public NotionController(INotionButtonClicker notionButtonClicker)
    {
        m_notionButtonClicker = notionButtonClicker;
    }
    
    [HttpGet]
    [Route("api/getStates")]
    public async Task<ActionResult<List<string>>> GetStates()
    {
        return await m_notionButtonClicker.GetStates();
    }


    [HttpGet]
    [Route("api/getTasks")]
    public async Task<ActionResult<List<TaskObject>>> GetTasks()
    {
        return await m_notionButtonClicker.GetTasks();
    }

    [HttpGet]
    [Route("api/updateTasks")]
    public async Task<ActionResult> UpdateTasks()
    {
        await m_notionButtonClicker.UpdateTasks();
      
       return Ok();
    }
}