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
    public async Task<IEnumerable<GetCarsGridResponse>> Get(CancellationToken cancellationToken = default)
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

            select new GetCarsGridResponse
            {
                Id = c.Id,
                CapacityOverloadPenalty = c.CapacityOverloadPenalty,
                MaxCapacityOverloadPenalty = c.MaxCapacityOverloadPenalty,
                CarWorkHours = new BaseTimeWindowResponse
                {
                    WindowStart = c.WorkStart,
                    WindowEnd = c.WorkEnd
                },
                OverWorkPenalty = c.OverWorkPenalty,
                RouteTemplate = c.RouteTemplate,
                CarCapacities = capacities.ToList(),
            };

        return await query
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
