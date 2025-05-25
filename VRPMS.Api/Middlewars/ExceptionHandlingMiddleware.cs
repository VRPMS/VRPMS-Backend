using VRPMS.Common.Exceptions;

namespace VRPMS.Api.Middlewars;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var payload = new BusinessRuleResponse(ex);

            if (payload.Status is default(int))
            {
                payload.Status = StatusCodes.Status500InternalServerError;
            }

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
}