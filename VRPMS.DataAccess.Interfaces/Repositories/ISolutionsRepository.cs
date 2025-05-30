using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ISolutionsRepository
{
    Task<BaseSolutionResponse?> GetCurrentSolution(CancellationToken cancellationToken = default);

    Task<IEnumerable<GetSolutionRouteResponse>> GetSolutionRoutes(int solutionId, CancellationToken cancellationToken = default);

    Task<int?> CreateSolution(SolutionDto solution, CancellationToken cancellationToken = default);

    Task<int?> CreateSolutionRoute(SolutionRouteDto solution, CancellationToken cancellationToken = default);

    Task<bool> SolutionRouteVisitsBulkCopy(List<SolutionRouteVisitDto> solutionRouteVisits, CancellationToken cancellationToken = default);
}
