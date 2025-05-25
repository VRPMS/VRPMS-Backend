using Microsoft.AspNetCore.Mvc;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataContracts.Constants.ControllerConstants;
using VRPMS.DataContracts.Responses;

namespace VRPMS.Api.Controllers;

[ApiController]
[Route(CarsControllerConstants.Prefix)]
public class CarsController(
    ICarsService carsService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<GetCarsGridResponse>> GetCarsGrids()
    {
        return await carsService.GetCarsGrid();
    }
}
