using Microsoft.AspNetCore.Mvc;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataContracts.Constants.ControllerConstants;

namespace VRPMS.Api.Controllers;

[ApiController]
[Route(DataControllerConstants.Prefix)]
public class DataController(
    IDataService dataService)
    : ControllerBase
{
    [HttpPost(DataControllerConstants.ImportDataSuffix)]
    [Consumes("multipart/form-data")]
    public async Task ImportData([FromForm] IFormFile file)
    {
        await dataService.ImportData(file.OpenReadStream());
    }
}
