using Microsoft.AspNetCore.Mvc;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataContracts.Constants.ControllerConstants;
using VRPMS.DataContracts.Responses;

namespace VRPMS.Api.Controllers;

[ApiController]
public class LovsController(
    ILovsService lovsService)
    : ControllerBase
{
    [HttpGet(LovsControllerConstants.GetLocationsTypesLovRoute)]
    public async Task<IEnumerable<BaseTypeResponse>> GetLocationTypesLov()
    {
        return await lovsService.GetLocationTypesLov();
    }

    [HttpGet(LovsControllerConstants.GetDemandTypesLovRoute)]
    public async Task<IEnumerable<BaseTypeResponse>> GetDemandTypesLov()
    {
        return await lovsService.GetDemandTypesLov();
    }

    [HttpGet(LovsControllerConstants.GetCarTypesLovRoute)]
    public async Task<IEnumerable<BaseTypeResponse>> GetCarTypesLov()
    {
        return await lovsService.GetCarTypesLov();
    }
}
