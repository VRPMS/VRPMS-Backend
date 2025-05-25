using LinqToDB.Data;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Interfaces.Functions;

internal class VrpmsProcedures (
    AppDataConnection db)
    : IVrpmsProcedures
{
    public async Task TruncateTables()
    {
        const string procedureName = "prc_truncate_tables";

        await db.ExecuteProcAsync($"{DbConstants.Schema}.{procedureName}");
    }
}
