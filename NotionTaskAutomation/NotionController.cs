using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
    [Route("api/updateTimetable")]
    public async Task<ActionResult<string>> ExecuteTimeTableUpdate()
    {
        return await m_notionButtonClicker.ExecuteClickAsync();
    }
}