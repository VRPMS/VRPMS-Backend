using ExcelDataReader;
using System.Data;
using System.Xml.Linq;
using VRPMS.BusinessLogic.Constants;
using VRPMS.BusinessLogic.Validators.BusinessValidators;
using VRPMS.Common.Exceptions;
using VRPMS.DataAccess.Interfaces.Constants;
using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.VRPCD.Enums;

namespace VRPMS.BusinessLogic.Helpers;

internal class ExcelParser(ExcelValidator excelValidator)
{
    public async Task<DataDto> ParseDataFromExcel(Stream fileStream)
    {
        await excelValidator.ValidateExcelFile(fileStream);
        using var reader = ExcelReaderFactory.CreateReader(fileStream);

        var ds = await Task.Run(() => reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false
            }
        }));

        var demandTypes = GetDemandTypeDtos(ds);
        int timeWindowsCount = GetTimeWindowsCount(ds);

        var tables = GetAndValidateDataTables(ds, demandTypes.Count, timeWindowsCount);

        var locations = GetLocations(
            tables[ExcelTableNames.Points], 
            tables[ExcelTableNames.CrossDockWarehouse], 
            demandTypes.Count, 
            timeWindowsCount);

        return new DataDto
        {
            DemandTypes = demandTypes,
            Locations = locations,
            LocationDemands = GetLocationDemands(tables[ExcelTableNames.Points], demandTypes.Count),
            LocationTimeWindows = GetLocationTimeWindows(tables[ExcelTableNames.Points], demandTypes.Count, timeWindowsCount),
            LocationRoutes = GetLocationRoutes(tables[ExcelTableNames.Routes], locations),
            LocationSupplyChains = GetLocationSupplyChains(tables[ExcelTableNames.CrossDockPoints], locations),
            Cars = GetCars(tables[ExcelTableNames.Cars], demandTypes.Count, locations),
            CarCapacities = GetCarCapacities(tables[ExcelTableNames.Cars], demandTypes.Count)
        };
    }

    private List<DemandTypeDto> GetDemandTypeDtos(DataSet ds)
    {
        var table = excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.DemandSize, x => x.Rows.Count == 1 && x.Columns.Count == 1);

        int demandSize = excelValidator.GetValueAndCheckType<int>(table.Rows[0][0], ExcelTableNames.DemandSize, x => x > 0);

        List<DemandTypeDto> demandTypes = [];

        for (int i = 1; i <= demandSize; i++)
        {
            demandTypes.Add(new DemandTypeDto
            {
                Id = i,
                Name = $"Demand {i}"
            });
        }

        return demandTypes;
    }

    private int GetTimeWindowsCount(DataSet ds)
    {
        var table = excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.TimeWindows, x => x.Rows.Count == 1 && x.Columns.Count == 1);

        int timeWindowsCount = excelValidator.GetValueAndCheckType<int>(table.Rows[0][0], ExcelTableNames.TimeWindows, x => x > 0);

        return timeWindowsCount;
    }

    private Dictionary<string, DataTable> GetAndValidateDataTables(DataSet ds, int demandsCount, int timeWindowsCount)
    {
        return new Dictionary<string, DataTable>
        {
            { 
                ExcelTableNames.Points, 
                excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.Points, x => x.Columns.Count == 6 + demandsCount + timeWindowsCount * 2) 
            },
            { 
                ExcelTableNames.CrossDockWarehouse, 
                excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.CrossDockWarehouse, x => x.Columns.Count == 2) 
            },
            { 
                ExcelTableNames.CrossDockPoints, 
                excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.CrossDockPoints, x => x.Columns.Count == 3) 
            },
            { 
                ExcelTableNames.Routes, 
                excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.Routes, x => x.Columns.Count == 4) 
            },
            { 
                ExcelTableNames.Cars, 
                excelValidator.GetTableAndCheckExists(ds, ExcelTableNames.Cars, x => x.Columns.Count == 7 + demandsCount * 2) 
            }
        };
    }

    private List<LocationDto> GetLocations(
        DataTable pointsTable,
        DataTable crossDockWarehouseTable,
        int demandsCount,
        int timeWindowsCount)
    {
        const double minLatitude = -90.0;
        const double maxLatitude = 90.0;
        const double minLongitude = 0.0;
        const double maxLongitude = 180.0;

        var crossDockWarehouses = crossDockWarehouseTable.Rows
            .Cast<DataRow>()
            .ToDictionary(
                row => excelValidator.GetValueAndCheckType<int>(row[0], crossDockWarehouseTable.TableName),
                row => excelValidator.GetValueAndCheckType<int>(row[1], crossDockWarehouseTable.TableName)
            );

        List<LocationDto> locations = [];

        foreach (DataRow row in pointsTable.Rows)
        {
            var id = excelValidator.GetValueAndCheckType<int>(row[0], pointsTable.TableName);

            int? type = crossDockWarehouses.TryGetValue(id, out var t) ? t : null;

            int pointType = type switch
            {
                1 => (int)LocationTypeEnum.Warehouse,
                0 => (int)LocationTypeEnum.CrossDock,
                _ => (int)LocationTypeEnum.Client
            };

            var nextIndex = 3 + demandsCount + timeWindowsCount * 2;

            locations.Add(new LocationDto
            {
                Id = id,
                LocationTypeId = pointType,
                Latitude = excelValidator.GetValueAndCheckType<double>(row[1], pointsTable.TableName, x => x >= minLatitude && x <= maxLatitude),
                Longitude = excelValidator.GetValueAndCheckType<double>(row[2], pointsTable.TableName, x => x >= minLongitude && x <= maxLongitude),
                ServiceTime = TimeSpan.FromSeconds(excelValidator.GetValueAndCheckType<double>(row[nextIndex], pointsTable.TableName, x => x >= 0)),
                LatePenalty = excelValidator.GetValueAndCheckType<int>(row[nextIndex + 1], pointsTable.TableName, x => x >= 0),
                WaitPenalty = excelValidator.GetValueAndCheckType<int>(row[nextIndex + 2], pointsTable.TableName, x => x >= 0)
            });
        }

        return locations;
    }

    private List<LocationDemandDto> GetLocationDemands(DataTable pointsTable, int demandsCount)
    {
        List<LocationDemandDto> locationDemands = [];

        foreach (DataRow row in pointsTable.Rows)
        {
            var id = excelValidator.GetValueAndCheckType<int>(row[0], pointsTable.TableName);

            var nextIndex = 3;

            for (int i = 0; i < demandsCount; i++)
            {
                double demandValue = excelValidator.GetValueAndCheckType<double>(row[nextIndex + i], pointsTable.TableName, x => x >= 0);

                if (demandValue > 0)
                {
                    locationDemands.Add(new LocationDemandDto
                    {
                        LocationId = id,
                        DemandId = i + 1,
                        DemandValue = demandValue
                    });
                }
            }
        }

        return locationDemands;
    }

    private List<LocationTimeWindowDto> GetLocationTimeWindows(DataTable pointsTable, int demandsCount, int timeWindowsCount)
    {
        const int minTime = 0;
        const int maxTime = 86399;

        List<LocationTimeWindowDto> locationTimeWindows = [];

        foreach (DataRow row in pointsTable.Rows)
        {
            var id = excelValidator.GetValueAndCheckType<int>(row[0], pointsTable.TableName);

            var nextIndex = 3 + demandsCount;

            for (int i = 0; i < timeWindowsCount; i++)
            {
                int start = excelValidator.GetValueAndCheckType<int>(row[nextIndex + i * 2], pointsTable.TableName);
                int end = excelValidator.GetValueAndCheckType<int>(row[nextIndex + i * 2 + 1], pointsTable.TableName, x => x >= start);

                if (start > end)
                {
                    throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, pointsTable.TableName));
                }

                if (start < end)
                {
                    locationTimeWindows.Add(new LocationTimeWindowDto
                    {
                        LocationId = id,
                        WindowStart = start < minTime ? null : TimeSpan.FromSeconds(start),
                        WindowEnd = end > maxTime ? null : TimeSpan.FromSeconds(end)
                    });
                }
            }
        }

        return locationTimeWindows;
    }

    private List<CarDto> GetCars(DataTable carsTable, int demandsCount, List<LocationDto> locations)
    {
        const int minTime = 0;
        const int maxTime = 86399;

        List<CarDto> cars = [];

        foreach (DataRow row in carsTable.Rows)
        {
            var id = excelValidator.GetValueAndCheckType<int>(row[0], ExcelTableNames.Cars);

            var capacityNextIndex = 1 + demandsCount;
            var maxCapacityNextIndex = 1 + demandsCount + 5 + demandsCount;

            int workStart = excelValidator.GetValueAndCheckType<int>(row[capacityNextIndex], ExcelTableNames.Cars);
            int workEnd = excelValidator.GetValueAndCheckType<int>(row[capacityNextIndex + 1], ExcelTableNames.Cars, x => x >= workStart);

            if (workStart > workEnd)
            {
                throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.Cars));
            }

            var routeStartPointId = excelValidator.GetValueAndCheckType<int>(row[capacityNextIndex + 3], ExcelTableNames.Cars);

            if (!locations.Any(x => x.Id == routeStartPointId))
            {
                throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.Cars));
            }

            cars.Add(new CarDto
            {
                Id = id,
                CapacityOverloadPenalty = excelValidator.GetValueAndCheckType<int>(row[capacityNextIndex + 2], ExcelTableNames.Cars, x => x >= 0),
                MaxCapacityOverloadPenalty = excelValidator.GetValueAndCheckType<int>(row[maxCapacityNextIndex], ExcelTableNames.Cars, x => x >= 0),
                WorkStart = workStart < minTime || workStart == workEnd ? null : TimeSpan.FromSeconds(workStart),
                WorkEnd = workEnd > maxTime || workStart == workEnd ? null : TimeSpan.FromSeconds(workEnd),
                OverWorkPenalty = excelValidator.GetValueAndCheckType<int>(row[capacityNextIndex + 4], ExcelTableNames.Cars, x => x >= 0),
                RouteStartLocationId = routeStartPointId
            });
        }

        return cars;
    }

    private List<CarCapacityDto> GetCarCapacities(DataTable carsTable, int demandsCount)
    {
        List<CarCapacityDto> carCapacities = [];

        foreach (DataRow row in carsTable.Rows)
        {
            var id = excelValidator.GetValueAndCheckType<int>(row[0], ExcelTableNames.Cars);

            var capacityNextIndex = 1;
            var maxCapacityNextIndex = 1 + demandsCount + 5;

            for (int i = 0; i < demandsCount; i++)
            {
                double capacity = excelValidator.GetValueAndCheckType<double>(row[capacityNextIndex + i], ExcelTableNames.Cars, x => x >= 0);
                double maxCapacity = excelValidator.GetValueAndCheckType<double>(row[maxCapacityNextIndex + i], ExcelTableNames.Cars, x => x >= 0);

                if (capacity > 0)
                {
                    carCapacities.Add(new CarCapacityDto
                    {
                        CarId = id,
                        DemandId = i + 1,
                        Capacity = capacity,
                        MaxCapacity = maxCapacity
                    });
                }
            }
        }

        return carCapacities;
    }

    private List<LocationRouteDto> GetLocationRoutes(DataTable routesTable, List<LocationDto> locations)
    {
        List<LocationRouteDto> routes = [];

        foreach (DataRow row in routesTable.Rows)
        {
            int fromPointId = excelValidator.GetValueAndCheckType<int>(row[0], routesTable.TableName);
            int toPointId = excelValidator.GetValueAndCheckType<int>(row[1], routesTable.TableName);

            int duration = excelValidator.GetValueAndCheckType<int>(row[2], routesTable.TableName, x => x >= 0);
            double distance = excelValidator.GetValueAndCheckType<double>(row[3], routesTable.TableName, x => x >= 0);

            routes.Add(new LocationRouteDto
            {
                FromPointId = locations[fromPointId].Id,
                ToPointId = locations[toPointId].Id,
                Duration = TimeSpan.FromSeconds(duration),
                Distance = distance
            });
        }
        return routes;
    }

    private List<LocationSupplyChainDto> GetLocationSupplyChains(DataTable supplyChainsTable, List<LocationDto> locations)
    {
        List<LocationSupplyChainDto> supplyChains = [];

        foreach (DataRow row in supplyChainsTable.Rows)
        {
            int clientId = excelValidator.GetValueAndCheckType<int>(row[0], ExcelTableNames.CrossDockPoints, x => x != 0);
            int? warehouseId = null;
            int? crossDockId = null;

            if (!locations.Any(x => x.Id == clientId && x.LocationTypeId == (int)LocationTypeEnum.Client))
            {
                throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
            }

            int supplier = excelValidator.GetValueAndCheckType<int>(row[1], ExcelTableNames.CrossDockPoints);

            if (supplier != 0)
            {
                var supplierType = locations.FirstOrDefault(x => x.Id == supplier)?.LocationTypeId;

                if (supplierType is null)
                {
                    throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
                }
                else
                {
                    if (supplierType == (int)LocationTypeEnum.Warehouse)
                    {
                        warehouseId = supplier;
                    }
                    else if (supplierType == (int)LocationTypeEnum.CrossDock)
                    {
                        crossDockId = supplier;
                    }
                    else
                    {
                        throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
                    }
                }
            }

            supplier = excelValidator.GetValueAndCheckType<int>(row[2], ExcelTableNames.CrossDockPoints);

            if (supplier != 0)
            {
                var supplierType = locations.FirstOrDefault(x => x.Id == supplier)?.LocationTypeId;

                if (supplierType is null)
                {
                    throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
                }
                else
                {
                    if (supplierType == (int)LocationTypeEnum.Warehouse)
                    {
                        if (warehouseId.HasValue)
                        {
                            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
                        }

                        warehouseId = supplier;
                    }
                    else if (supplierType == (int)LocationTypeEnum.CrossDock)
                    {
                        if (crossDockId.HasValue)
                        {
                            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
                        }

                        crossDockId = supplier;
                    }
                    else
                    {
                        throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
                    }
                }
            }

            supplyChains.Add(new LocationSupplyChainDto
            {
                ClientId = clientId,
                WarehouseId = warehouseId,
                CrossDockId = crossDockId
            });
        }

        //if (supplyChains.Count != locations.Count(x => x.LocationTypeId == (int)LocationTypeEnum.Client))
        //{
        //    throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, $"{ExcelTableNames.Points}' and/or '{ExcelTableNames.CrossDockPoints}"));
        //}

        return supplyChains;
    }
}
