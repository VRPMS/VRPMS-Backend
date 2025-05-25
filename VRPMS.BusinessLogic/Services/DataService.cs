using VRPMS.BusinessLogic.Helpers;
using VRPMS.BusinessLogic.Interfaces.Services;

namespace VRPMS.BusinessLogic.Services;

internal class DataService(
    ExcelParser excelParser)
    : IDataService
{
    public async Task ImportData(Stream fileStream)
    {
        var dataDto = await excelParser.ParseDataFromExcel(fileStream);
    }
}
