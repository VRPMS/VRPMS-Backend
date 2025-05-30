using LinqToDB;
using LinqToDB.Data;
using VRPMS.DataAccess.Entities;
using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Repositories;

internal class SolutionsRepository(
    AppDataConnection db)
    : ISolutionsRepository
{
    public async Task<BaseSolutionResponse?> GetCurrentSolution(CancellationToken cancellationToken = default)
    {
        var query = db.Solutions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new BaseSolutionResponse
            {
                Id = s.Id,
                TotalScore = s.TotalScore
            });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<GetSolutionRouteResponse>> GetSolutionRoutes(int solutionId, CancellationToken cancellationToken = default)
    {
        var query = db.SolutionRoutes
            .Where(sr => sr.SolutionId == solutionId)
            .Select(sr => new GetSolutionRouteResponse
            {
                Id = sr.Id,
                CarId = sr.CarId,
                Visits = sr.SolutionRouteVisits
                    .OrderBy(srv => srv.SequenceNumber)
                    .Select(srv => new SolutionRouteVisitResponse
                    {
                        LocationId = srv.PointId,
                        ArrivalTime = srv.ArrivalTime,
                        DepartureTime = srv.DepartureTime
                    })
                    .ToList()
            });

        return await query
            .OrderBy(sr => sr.CarId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> CreateSolution(SolutionDto solution, CancellationToken cancellationToken = default)
    {
        return await db.Solutions
            .Value(s => s.TotalScore, solution.TotalScore)
            .InsertWithInt32IdentityAsync(cancellationToken);
    }

    public async Task<int?> CreateSolutionRoute(SolutionRouteDto solution, CancellationToken cancellationToken = default)
    {
        return await db.SolutionRoutes
            .Value(s => s.SolutionId, solution.SolutionId)
            .Value(s => s.CarId, solution.CarId)
            .InsertWithInt32IdentityAsync(cancellationToken);
    }

    public async Task<bool> SolutionRouteVisitsBulkCopy(List<SolutionRouteVisitDto> solutionRouteVisits, CancellationToken cancellationToken = default)
    {
        var result = await db.BulkCopyAsync(new BulkCopyOptions
        {
            KeepIdentity = false
        }, solutionRouteVisits.Select(x => new SolutionRouteVisit
        {
            SolutionRouteId = x.SolutionRouteId,
            SequenceNumber = x.SequenceNumber,
            PointId = x.LocationId,
            ArrivalTime = x.ArrivalTime,
            DepartureTime = x.DepartureTime
        }), cancellationToken);

        return !result.Abort && result.RowsCopied == solutionRouteVisits.Count;
    }
}
