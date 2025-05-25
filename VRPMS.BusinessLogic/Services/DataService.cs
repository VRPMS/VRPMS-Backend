using VRPMS.BusinessLogic.Constants;
using VRPMS.BusinessLogic.Helpers;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.Common.Exceptions;
using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataAccess.Interfaces.Functions;
using VRPMS.DataAccess.Interfaces.Repositories;

namespace VRPMS.BusinessLogic.Services;

internal class DataService(
    ExcelParser excelParser,
    IVrpmsFunctions vrpmsFunctions,
    IVrpmsProcedures vrpmsProcedures,
    IDemandsRepository demandsRepository,
    ILocationsRepository locationsRepository,
    ICarsRepository carsRepository)
    : IDataService
{
    public async Task ImportData(Stream fileStream)
    {
        DataDto dataDto = await excelParser.ParseDataFromExcel(fileStream);

        if (await vrpmsFunctions.HasAnyData())
        {
            await vrpmsProcedures.TruncateTables();

            if (await vrpmsFunctions.HasAnyData())
            {
                throw new BusinessException(BusinessErrorMessages.TruncateTablesFailed);
            }
        }

        await RunPhaseAsync(
            (nameof(dataDto.DemandTypes), () => demandsRepository.DemandsBulkCopy(dataDto.DemandTypes)),
            (nameof(dataDto.Locations), () => locationsRepository.LocationsBulkCopy(dataDto.Locations)),
            (nameof(dataDto.Cars), () => carsRepository.CarsBulkCopy(dataDto.Cars))
        );

        await RunPhaseAsync(
            (nameof(dataDto.LocationDemands), () => locationsRepository.LocationDemandsBulkCopy(dataDto.LocationDemands)),
            (nameof(dataDto.LocationTimeWindows), () => locationsRepository.LocationTimeWindowsBulkCopy(dataDto.LocationTimeWindows)),
            (nameof(dataDto.LocationRoutes), () => locationsRepository.LocationRoutesBulkCopy(dataDto.LocationRoutes)),
            (nameof(dataDto.LocationSupplyChains), () => locationsRepository.LocationSupplyChainsBulkCopy(dataDto.LocationSupplyChains)),
            (nameof(dataDto.CarCapacities), () => carsRepository.CarCapacitiesBulkCopy(dataDto.CarCapacities))
        );
    }

    private async Task RunPhaseAsync(params (string Name, Func<Task<bool>> Operation)[] operations)
    {
        foreach (var operation in operations)
        {
            var success = await operation.Operation();

            if (!success)
            {
                throw new BusinessException(string.Format(BusinessErrorMessages.DataCopyFailed, operation.Name));
            }
        }
    }
}
 