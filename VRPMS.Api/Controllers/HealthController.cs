using Microsoft.AspNetCore.Mvc;
using VRPMS.DataContracts.Constants.ControllerConstants;

namespace VRPMS.Api.Controllers;

[Route(HealthControllerConstants.Prefix)]
[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(DateTime.Now);
    }
}
