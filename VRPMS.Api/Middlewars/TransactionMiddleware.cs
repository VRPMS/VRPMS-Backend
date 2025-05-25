using VRPMS.DataAccess;

namespace VRPMS.Api.Middlewars;

public class TransactionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDataConnection db)
    {
        await using var transaction = await db.BeginTransactionAsync();

        try
        {
            await next(context);

            if (context.Response.StatusCode < 400)
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
