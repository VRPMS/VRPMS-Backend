using LinqToDB.Data;
using VRPMS.DataAccess.Constants;
using VRPMS.DataAccess.Interfaces.Functions;

namespace VRPMS.DataAccess.Functions;

internal class VrpmsFunctions(
    AppDataConnection db)
    : IVrpmsFunctions
{
    public async Task<bool> HasAnyData()
    {
        const string functionName = "fn_has_any_data";

        return await db.ExecuteAsync<bool>($"SELECT {DbConstants.Schema}.{functionName}()");
    }
}
