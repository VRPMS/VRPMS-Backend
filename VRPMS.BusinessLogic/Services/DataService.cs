using VRPMS.BusinessLogic.Constants;
using VRPMS.BusinessLogic.Helpers;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.Common.Exceptions;
using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataAccess.Interfaces.Functions;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.VRPCD;
using VRPMS.VRPCD.Methods.BasicSolutionMethods;

namespace VRPMS.BusinessLogic.Services;

internal class DataService(
    ExcelParser excelParser,
    IVrpmsFunctions vrpmsFunctions,
    IVrpmsProcedures vrpmsProcedures,
    IDemandsRepository demandsRepository,
    ILocationsRepository locationsRepository,
    ICarsRepository carsRepository,
    ISolutionsRepository solutionsRepository)
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

        // Order of operations is important to maintain foreign key constraints
        await RunPhaseAsync(
            (nameof(dataDto.DemandTypes), () => demandsRepository.DemandsBulkCopy(dataDto.DemandTypes)),
            (nameof(dataDto.Locations), () => locationsRepository.LocationsBulkCopy(dataDto.Locations)),
            (nameof(dataDto.LocationDemands), () => locationsRepository.LocationDemandsBulkCopy(dataDto.LocationDemands)),
            (nameof(dataDto.LocationTimeWindows), () => locationsRepository.LocationTimeWindowsBulkCopy(dataDto.LocationTimeWindows)),
            (nameof(dataDto.LocationRoutes), () => locationsRepository.LocationRoutesBulkCopy(dataDto.LocationRoutes)),
            (nameof(dataDto.LocationSupplyChains), () => locationsRepository.LocationSupplyChainsBulkCopy(dataDto.LocationSupplyChains)),
            (nameof(dataDto.Cars), () => carsRepository.CarsBulkCopy(dataDto.Cars)),
            (nameof(dataDto.CarCapacities), () => carsRepository.CarCapacitiesBulkCopy(dataDto.CarCapacities))
        );

        Solver solver = new(basicSolver: new NearestNeighborMethod());

        SolutionDto solutionDto = VrpcdHelper.GetSolutionDto(solver.Solve(VrpcdHelper.GetProblem(dataDto)));

        var solutionId = await solutionsRepository.CreateSolution(solutionDto);

        if (!solutionId.HasValue)
        {
            throw new BusinessException(BusinessErrorMessages.SolutionCreationFailed);
        }

        foreach (var solutionRoute in solutionDto.SolutionRoutes)
        {
            solutionRoute.SolutionId = solutionId.Value;

            var solutionRouteId = await solutionsRepository.CreateSolutionRoute(solutionRoute);

            if (!solutionRouteId.HasValue)
            {
                throw new BusinessException(BusinessErrorMessages.SolutionRouteCreationFailed);
            }

            foreach (var solutionRouteVisit in solutionRoute.SolutionRouteVisits)
            {
                solutionRouteVisit.SolutionRouteId = solutionRouteId.Value;
            }

            await RunPhaseAsync((nameof(solutionRoute.SolutionRouteVisits), () => solutionsRepository.SolutionRouteVisitsBulkCopy(solutionRoute.SolutionRouteVisits)));
        }
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
 