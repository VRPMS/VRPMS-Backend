using Microsoft.AspNetCore.Mvc;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataContracts.Constants.ControllerConstants;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.Api.Controllers;

[ApiController]
[Route(LocationsControllerConstants.Prefix)]
public class LocationsController(
    ILocationsService locationsService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<GetLocationsGridResponse>> GetLocationsGrid([FromQuery] GetLocationsGridRequest request)
    {
        return await locationsService.GetLocationsGrid(request);
    }
}