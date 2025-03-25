using Microsoft.AspNetCore.Mvc;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataContracts.Constants.ControllerConstants;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.Api.Controllers;

[ApiController]
[Route(TestControllerConstants.Prefix)]
public class TestController(
    ITestService testService)
    : ControllerBase
{
    [HttpGet]
    public async Task<TestResponse> Get([FromQuery] TestRequest request)
    {
        return await testService.GetTestResponse(request);
    }
}