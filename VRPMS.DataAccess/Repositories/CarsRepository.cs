using LinqToDB;
using LinqToDB.Data;
using VRPMS.Common.Extensions;
using VRPMS.DataAccess.Entities;
using VRPMS.DataAccess.Interfaces.Dtos;
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

    public async Task<bool> CarsBulkCopy(List<CarDto> cars, CancellationToken cancellationToken = default)
    {
        var options = new BulkCopyOptions
        {
            KeepIdentity = true
        };

        BulkCopyRowsCopied result;
        long totalCount = 0;

        result = await db.BulkCopyAsync(
            options,
            cars
            .Where(x => x.WorkStart == null && x.WorkEnd == null)
            .Select(x => new Car
            {
                Id = x.Id,
                CapacityOverloadPenalty = x.CapacityOverloadPenalty,
                MaxCapacityOverloadPenalty = x.MaxCapacityOverloadPenalty,
                OverWorkPenalty = x.OverWorkPenalty,
                RouteTemplate = x.RouteTemplate
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;

        result = await db.BulkCopyAsync(
            options,
            cars
            .Where(x => x.WorkStart != null && x.WorkEnd == null)
            .Select(x => new Car
            {
                Id = x.Id,
                CapacityOverloadPenalty = x.CapacityOverloadPenalty,
                MaxCapacityOverloadPenalty = x.MaxCapacityOverloadPenalty,
                WorkStart = x.WorkStart!.Value,
                OverWorkPenalty = x.OverWorkPenalty,
                RouteTemplate = x.RouteTemplate
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;

        result = await db.BulkCopyAsync(
            options,
            cars
            .Where(x => x.WorkStart == null && x.WorkEnd != null)
            .Select(x => new Car
            {
                Id = x.Id,
                CapacityOverloadPenalty = x.CapacityOverloadPenalty,
                MaxCapacityOverloadPenalty = x.MaxCapacityOverloadPenalty,
                WorkEnd = x.WorkEnd!.Value,
                OverWorkPenalty = x.OverWorkPenalty,
                RouteTemplate = x.RouteTemplate
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;

        result = await db.BulkCopyAsync(
            options,
            cars
            .Where(x => x.WorkStart != null && x.WorkEnd != null)
            .Select(x => new Car
            {
                Id = x.Id,
                CapacityOverloadPenalty = x.CapacityOverloadPenalty,
                MaxCapacityOverloadPenalty = x.MaxCapacityOverloadPenalty,
                WorkStart = x.WorkStart!.Value,
                WorkEnd = x.WorkEnd!.Value,
                OverWorkPenalty = x.OverWorkPenalty,
                RouteTemplate = x.RouteTemplate
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;


        return totalCount == cars.Count;
    }

    public async Task<bool> CarCapacitiesBulkCopy(List<CarCapacityDto> carCapacities, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = false
        }, carCapacities.Select(x => new CarCapacity
        {
            CarId = x.CarId,
            DemandId = x.DemandId,
            Capacity = x.Capacity,
            MaxCapacity = x.MaxCapacity
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == carCapacities.Count;
    }
}
