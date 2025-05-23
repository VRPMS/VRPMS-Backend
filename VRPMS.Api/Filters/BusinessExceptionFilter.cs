using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VRPMS.Common.Exceptions;

namespace VRPMS.Api.Filters;

public class BusinessExceptionFilter : IExceptionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is BusinessException businessException)
        {
            var payload = new BusinessRuleResponse(businessException);

            if (payload.Status is default(int))
            {
                payload.Status = StatusCodes.Status400BadRequest;
            }

            context.Result = new ObjectResult(payload)
            {
                StatusCode = payload.Status,
                DeclaredType = payload.GetType()
            };

            context.ExceptionHandled = true;
        }
    }
}
