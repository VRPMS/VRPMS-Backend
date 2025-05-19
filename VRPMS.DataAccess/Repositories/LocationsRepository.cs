using LinqToDB;
using VRPMS.Common.Extensions;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Repositories;

internal class LocationsRepository(AppDataConnection db) : ILocationsRepository
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
}