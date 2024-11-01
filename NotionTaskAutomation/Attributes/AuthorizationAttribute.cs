namespace NotionTaskAutomation.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class Authorization : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.ContainsKey("Authorization")) // check if it's valid token
            context.Result = new UnauthorizedResult();

        base.OnActionExecuting(context);
    }
}