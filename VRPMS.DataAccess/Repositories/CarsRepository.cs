using LinqToDB;
using VRPMS.Common.Extensions;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Repositories;

internal class CarsRepository(
    AppDataConnection db)
    : ICarsRepository
{
    public async Task<IEnumerable<GetCarsGridResponse>> Get(GetCarsGridRequest request, CancellationToken cancellationToken = default)
    {
        var query =
            from c in db.Cars

            let capacities =
                from cc in c.CarCapacities
                orderby cc.DemandId
                select new BaseCarCapacityResponse
                {
                    DemandId = cc.DemandId,
                    DemandName = cc.Demand.Name,
                    Capacity = cc.Capacity,
                    MaxCapacity = cc.MaxCapacity
                }

            let timeWindows =
                from tw in c.CarTimeWindows
                orderby tw.Id
                select new BaseTimeWindowResponse
                {
                    WindowStart = tw.WindowStart,
                    WindowEnd = tw.WindowEnd
                }

            select new GetCarsGridResponse
            {
                Id = c.Id,
                CarType = new BaseTypeResponse
                {
                    TypeId = c.CarTypeId,
                    TypeName = c.CarType.Name
                },
                CapacityOverloadPenalty = c.CapacityOverloadPenalty,
                MaxCapacityOverloadPenalty = c.MaxCapacityOverloadPenalty,
                OverWorkPenalty = c.OverWorkPenalty,
                RouteTemplate = c.RouteTemplate,
                CarCapacities = capacities.ToList(),
                CarWorkHours = timeWindows.ToList()
            };

        query = query.WhereIfHasValue(request.CarTypeId, c => c.CarType.TypeId == c.CarType.TypeId);

        return await query
            .OrderBy(c => c.CarType.TypeId)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default)
    {
        var query =
            from ct in db.CarTypes
            select new BaseTypeResponse
            {
                TypeId = ct.Id,
                TypeName = ct.Name
            };

        return await query.ToListAsync(cancellationToken);
    }
}
