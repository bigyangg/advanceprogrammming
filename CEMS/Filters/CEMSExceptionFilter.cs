using CEMS.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CEMS.Filters;

public class CEMSExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not CEMSException cemsException)
        {
            return;
        }

        context.ModelState.AddModelError(string.Empty, cemsException.Message);
        context.Result = new BadRequestObjectResult(context.ModelState);

        context.ExceptionHandled = true;
    }
}
