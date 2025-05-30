using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Services;

internal class SolutionsService(
    ISolutionsRepository solutionsRepository,
    ILocationsRepository locationsRepository)
    : ISolutionsService
{
    public async Task<IEnumerable<GetSolutionRouteResponse>> GetCurrentSolutionRoutes()
    {
        var currentSolution = await solutionsRepository.GetCurrentSolution();

        if (currentSolution == null)
        {
            return [];
        }

        var solutionRoutes = await solutionsRepository.GetSolutionRoutes(currentSolution.Id);
        var routes = await locationsRepository.GetRoutes();

        foreach (var solution in solutionRoutes)
        {
            for (int i = 1; i < solution.Visits.Count; i++)
            {
                var locationRoute = routes.First(r =>
                    r.FromLocationId == solution.Visits[i - 1].LocationId
                    && r.ToLocationId == solution.Visits[i].LocationId);

                solution.Visits[i].Distance = locationRoute.Distance;
                solution.Visits[i].Duration = locationRoute.Duration;
            }
        }

        return solutionRoutes;
    }
}
