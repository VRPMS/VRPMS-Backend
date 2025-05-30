using Microsoft.AspNetCore.Mvc;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataContracts.Constants.ControllerConstants;
using VRPMS.DataContracts.Responses;

namespace VRPMS.Api.Controllers;

[ApiController]
[Route(SolutionsControllerConstants.Prefix)]
public class SolutionsController(
    ISolutionsService solutionsService)
    : ControllerBase
{
    [HttpGet(SolutionsControllerConstants.GetCurrentSolutionRoutesSuffix)]
    public async Task<IEnumerable<GetSolutionRouteResponse>> GetCurrentSolutionRoutes()
    {
        return await solutionsService.GetCurrentSolutionRoutes();
    }
}
