using LinqToDB;
using LinqToDB.Data;
using VRPMS.DataAccess.Entities;
using VRPMS.DataAccess.Interfaces.Dtos;
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

    public async Task<bool> DemandsBulkCopy(List<DemandTypeDto> demands, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = true
        }, demands.Select(x => new Demand
        {
            Id = x.Id,
            Name = x.Name,
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == demands.Count;
    }
}
