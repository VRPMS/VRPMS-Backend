using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Services;

internal class SolutionsService(
    ISolutionsRepository solutionsRepository)
    : ISolutionsService
{
    public async Task<IEnumerable<GetSolutionRouteResponse>> GetCurrentSolutionRoutes()
    {
        var currentSolution = await solutionsRepository.GetCurrentSolution();

        if (currentSolution == null)
        {
            return [];
        }

        return await solutionsRepository.GetSolutionRoutes(currentSolution.Id);
    }
}
