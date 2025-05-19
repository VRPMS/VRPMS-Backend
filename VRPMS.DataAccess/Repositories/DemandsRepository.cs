using LinqToDB;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Repositories;

internal class DemandsRepository(
    AppDataConnection db)
    : IDemandsRepository
{
    public async Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default)
    {
        var query =
            from d in db.Demands
            select new BaseTypeResponse
            {
                TypeId = d.Id,
                TypeName = d.Name,
            };

        return await query.ToListAsync(cancellationToken);
    }
}
