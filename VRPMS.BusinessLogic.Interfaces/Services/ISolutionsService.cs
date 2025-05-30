using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Interfaces.Services;

public interface ISolutionsService
{
    Task<IEnumerable<GetSolutionRouteResponse>> GetCurrentSolutionRoutes();
}
