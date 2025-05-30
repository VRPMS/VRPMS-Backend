using LinqToDB;
using LinqToDB.Data;
using VRPMS.Common.Extensions;
using VRPMS.DataAccess.Entities;
using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Repositories;

internal class LocationsRepository(
    AppDataConnection db)
    : ILocationsRepository
{
    public async Task<IEnumerable<GetLocationsGridResponse>> Get(GetLocationsGridRequest request, CancellationToken cancellationToken = default)
    {
        var query =
            from p in db.Points

            let demands =
                from pd in p.PointDemands
                orderby pd.DemandId
                select new BaseDemandResponse
                {
                    DemandId = pd.DemandId,
                    DemandName = pd.Demand.Name,
                    DemandValue = pd.DemandValue
                }

            let timeWindows =
                from tw in p.PointTimeWindows
                orderby tw.Id
                select new BaseTimeWindowResponse
                {
                    WindowStart = tw.WindowStart,
                    WindowEnd = tw.WindowEnd
                }

            select new GetLocationsGridResponse
            {
                Id = p.Id,
                PointType = new BaseTypeResponse
                {
                    TypeId = p.PointTypeId,
                    TypeName = p.PointType.Name
                },
                Longitude = p.Longitude,
                Latitude = p.Latitude,
                ServiceTime = p.ServiceTime,
                PenaltyLate = p.LatePenalty,
                PenaltyWait = p.WaitPenalty,
                Demands = demands.ToList(),
                TimeWindows = timeWindows.ToList()
            };

        query = query.WhereIfHasValue(request.PointTypeId, p => p.PointType.TypeId == request.PointTypeId);

        return await query
            .OrderBy(p => p.PointType.TypeId)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BaseRouteResponse>> GetRoutes(CancellationToken cancellationToken = default)
    {
        var query =
            from pr in db.PointRoutes
            select new BaseRouteResponse
            {
                FromLocationId = pr.FromPointId,
                ToLocationId = pr.ToPointId,
                Duration = pr.Duration,
                Distance = pr.Distance
            };

        return await query
            .OrderBy(pr => pr.FromLocationId)
            .ThenBy(pr => pr.ToLocationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default)
    {
        var query =
            from pt in db.PointTypes
            select new BaseTypeResponse
            {
                TypeId = pt.Id,
                TypeName = pt.Name
            };

        return await query.ToListAsync(cancellationToken);
    }
    
    public async Task<bool> LocationsBulkCopy(List<LocationDto> locations, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = true
        }, locations.Select(x => new Point
        {
            Id = x.Id,
            PointTypeId = x.LocationTypeId,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            ServiceTime = x.ServiceTime,
            LatePenalty = x.LatePenalty,
            WaitPenalty = x.WaitPenalty
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == locations.Count;
    }

    public async Task<bool> LocationDemandsBulkCopy(List<LocationDemandDto> locationDemands, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = false
        }, locationDemands.Select(x => new PointDemand
        {
            PointId = x.LocationId,
            DemandId = x.DemandId,
            DemandValue = x.DemandValue
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == locationDemands.Count;
    }

    public async Task<bool> LocationTimeWindowsBulkCopy(List<LocationTimeWindowDto> locationTimeWindows, CancellationToken cancellationToken = default)
    {
        var options = new BulkCopyOptions
        {
            KeepIdentity = false
        };

        BulkCopyRowsCopied result;
        long totalCount = 0;

        result = await db.BulkCopyAsync(
            options, 
            locationTimeWindows
            .Where(x => x.WindowStart == null && x.WindowEnd == null)
            .Select(x => new PointTimeWindow
            {
                PointId = x.LocationId
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;

        result = await db.BulkCopyAsync(
            options,
            locationTimeWindows
            .Where(x => x.WindowStart != null && x.WindowEnd == null)
            .Select(x => new PointTimeWindow
            {
                PointId = x.LocationId,
                WindowStart = x.WindowStart!.Value
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;

        result = await db.BulkCopyAsync(
            options,
            locationTimeWindows
            .Where(x => x.WindowStart == null && x.WindowEnd != null)
            .Select(x => new PointTimeWindow
            {
                PointId = x.LocationId,
                WindowEnd = x.WindowEnd!.Value
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;

        result = await db.BulkCopyAsync(
            options,
            locationTimeWindows
            .Where(x => x.WindowStart != null && x.WindowEnd != null)
            .Select(x => new PointTimeWindow
            {
                PointId = x.LocationId,
                WindowStart = x.WindowStart!.Value,
                WindowEnd = x.WindowEnd!.Value
            }), cancellationToken);

        if (result.Abort)
        {
            return false;
        }

        totalCount += result.RowsCopied;


        return totalCount == locationTimeWindows.Count;
    }

    public async Task<bool> LocationRoutesBulkCopy(List<LocationRouteDto> locationRoutes, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = false
        }, locationRoutes.Select(x => new PointRoute
        {
            FromPointId = x.FromPointId,
            ToPointId = x.ToPointId,
            Duration = x.Duration,
            Distance = x.Distance
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == locationRoutes.Count;
    }

    public async Task<bool> LocationSupplyChainsBulkCopy(List<LocationSupplyChainDto> locationSupplyChains, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = false
        }, locationSupplyChains.Select(x => new PointSupplyChain
        {
            ClientId = x.ClientId,
            WarehouseId = x.WarehouseId,
            CrossDockId = x.CrossDockId
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == locationSupplyChains.Count;
    }
}