using System.Data;
using ExcelDataReader;
using VRPMS.BusinessLogic.Constants;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.Common.Enums;
using VRPMS.Common.Exceptions;
using VRPMS.DataAccess.Interfaces.Dtos;

namespace VRPMS.BusinessLogic.Services;

internal class DataService : IDataService
{
    public async Task ImportData(Stream fileStream)
    {
        if (fileStream.Length == 0)
        {
            throw new BusinessException(BusinessErrorMessages.FileIsEmpty);
        }

        if (!IsExcel(fileStream))
        {
            throw new BusinessException(BusinessErrorMessages.FileIsNotExcel);
        }

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(fileStream);

        var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false
            }
        });

        var demandTypes = GetDemandTypeDtos(ds);
        int timeWindowsCount = GetTimeWindowsCount(ds);
        var locations = GetLocations(ds, demandTypes.Count, timeWindowsCount);
        var cars = GetCars(ds, demandTypes.Count, locations.Locations);

        UpdateDataDto updateTableDto = new UpdateDataDto
        {
            DemandTypes = demandTypes,
            Locations = locations.Locations,
            LocationDemands = locations.LocationDemands,
            LocationsTimeWindows = locations.LocationTimeWindows,
            LocationSupplyChains = GetLocationSupplyChains(ds, locations.Locations),
            LocationRoutes = GetLocationRoutes(ds, locations.Locations),
            Cars = cars.Cars,
            CarCapacities = cars.CarCapacities
        };
    }

    private (
        List<CarDto> Cars,
        List<CarCapacityDto> CarCapacities)
        GetCars(DataSet ds, int demandsCount, List<LocationDto> locations)
    {
        const int minTime = 0;
        const int maxTime = 86399;
        const string asterisk = "*";

        var table = GetTableAndCheckExists(ds, ExcelTableNames.Cars);

        List<CarDto> cars = [];
        List<CarCapacityDto> carCapacities = [];

        foreach (DataRow row in table.Rows)
        {
            var id = GetValueAndCheckInteger(row[0], ExcelTableNames.Cars);

            var capacityNextIndex = 1;
            var maxCapacityNextIndex = 1 + demandsCount + 5;

            for (int i = 0; i < demandsCount; i++)
            {
                double capacity = GetValueAndCheckDouble(row[capacityNextIndex + i], ExcelTableNames.Cars, x => x >= 0);
                double maxCapacity = GetValueAndCheckDouble(row[maxCapacityNextIndex + i], ExcelTableNames.Cars, x => x >= 0);

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

            capacityNextIndex += demandsCount;
            maxCapacityNextIndex += demandsCount;

            int workStart = GetValueAndCheckInteger(row[capacityNextIndex], ExcelTableNames.Cars);
            int workEnd = GetValueAndCheckInteger(row[capacityNextIndex + 1], ExcelTableNames.Cars, x => x >= workStart);

            if (workStart > workEnd)
            {
                throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.Cars));
            }

            List<int?> routeTemplate = [];
            var strRouteTemplate = row[capacityNextIndex + 3].ToString()?.Trim(' ', '(', ')');

            if (!string.IsNullOrEmpty(strRouteTemplate))
            {
                var nodes = strRouteTemplate.Split(';');

                foreach (var node in nodes)
                {
                    if (node == asterisk)
                    {
                        routeTemplate.Add(null);
                    }
                    else
                    {
                        if (int.TryParse(node, out int nodeInt) && locations.Any(x => x.Id == nodeInt))
                        {
                            routeTemplate.Add(nodeInt);
                        }
                        else
                        {
                            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.Cars));
                        }
                    }
                }
            }

            cars.Add(new CarDto
            {
                Id = id,
                CapacityOverloadPenalty = GetValueAndCheckInteger(row[capacityNextIndex + 2], ExcelTableNames.Cars, x => x >= 0),
                MaxCapacityOverloadPenalty = GetValueAndCheckInteger(row[maxCapacityNextIndex], ExcelTableNames.Cars, x => x >= 0),
                WorkStart = workStart < minTime || workStart == workEnd ? null : TimeSpan.FromSeconds(workStart),
                WorkEnd = workEnd > maxTime || workStart == workEnd ? null : TimeSpan.FromSeconds(workEnd),
                OverWorkPenalty = GetValueAndCheckInteger(row[capacityNextIndex + 4], ExcelTableNames.Cars, x => x >= 0),
                RouteTemplate = routeTemplate
            });
        }

        return (cars, carCapacities);
    }

    private List<LocationRouteDto> GetLocationRoutes(DataSet ds, List<LocationDto> locations)
    {
        var table = GetTableAndCheckExists(ds, ExcelTableNames.Routes);

        List<LocationRouteDto> routes = [];

        foreach (DataRow row in table.Rows)
        {
            int fromPointId = GetValueAndCheckInteger(row[0], ExcelTableNames.Routes);
            int toPointId = GetValueAndCheckInteger(row[1], ExcelTableNames.Routes);

            double duration = GetValueAndCheckInteger(row[2], ExcelTableNames.Routes, x => x >= 0);
            double distance = GetValueAndCheckDouble(row[3], ExcelTableNames.Routes, x => x >= 0);

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

    private List<LocationSupplyChainDto> GetLocationSupplyChains(DataSet ds, List<LocationDto> locations)
    {
        var table = GetTableAndCheckExists(ds, ExcelTableNames.CrossDockPoints);
        
        List<LocationSupplyChainDto> supplyChains = [];
        
        foreach (DataRow row in table.Rows)
        {
            int clientId = GetValueAndCheckInteger(row[0], ExcelTableNames.CrossDockPoints, x => x != 0);
            int? warehouseId = null;
            int? crossDockId = null;

            if (!locations.Any(x => x.Id == clientId && x.PointTypeId == (int)LocationTypeEnum.Client))
            {
                throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.CrossDockPoints));
            }

            int supplier = GetValueAndCheckInteger(row[1], ExcelTableNames.CrossDockPoints);

            if (supplier != 0)
            {
                var supplierType = locations.FirstOrDefault(x => x.Id == supplier)?.PointTypeId;

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

            supplier = GetValueAndCheckInteger(row[2], ExcelTableNames.CrossDockPoints);

            if (supplier != 0)
            {
                var supplierType = locations.FirstOrDefault(x => x.Id == supplier)?.PointTypeId;

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

        return supplyChains;
    }

    private (
        List<LocationDto> Locations, 
        List<LocationDemandDto> LocationDemands, 
        List<LocationTimeWindowDto> LocationTimeWindows) 
        GetLocations(DataSet ds, int demandsCount, int timeWindowsCount)
    {
        const double minLatitude = -90.0;
        const double maxLatitude = 90.0;
        const double minLongitude = 0.0;
        const double maxLongitude = 180.0;
        const int minTime = 0;
        const int maxTime = 86399;

        var locationTable = GetTableAndCheckExists(ds, ExcelTableNames.Points);
        var crossDockWarehouseTable = GetTableAndCheckExists(ds, ExcelTableNames.CrossDockWarehouse);

        var crossDockWarehouses = crossDockWarehouseTable.Rows
            .Cast<DataRow>()
            .ToDictionary(
                row => GetValueAndCheckInteger(row[0], ExcelTableNames.CrossDockWarehouse),
                row => GetValueAndCheckInteger(row[1], ExcelTableNames.CrossDockWarehouse)
            );


        List<LocationDto> locations = [];
        List<LocationDemandDto> locationDemands = [];
        List<LocationTimeWindowDto> locationTimeWindows = [];

        foreach (DataRow row in locationTable.Rows)
        {
            var id = GetValueAndCheckInteger(row[0], ExcelTableNames.Points);

            int? type = crossDockWarehouses.TryGetValue(id, out var t) ? t : null;

            int pointType = type switch
            {
                1 => (int)LocationTypeEnum.Warehouse,
                0 => (int)LocationTypeEnum.CrossDock,
                _ => (int)LocationTypeEnum.Client
            };

            for (int i = 0; i < demandsCount; i++)
            {
                double demandValue = GetValueAndCheckDouble(row[3 + i], ExcelTableNames.Points, x => x >= 0);

                if (demandValue > 0)
                {
                    locationDemands.Add(new LocationDemandDto
                    {
                        PointId = id,
                        DemandId = i + 1,
                        DemandValue = demandValue
                    });
                }
            }

            var nextIndex = 3 + demandsCount;

            for (int i = 0; i < timeWindowsCount; i++)
            {
                int start = GetValueAndCheckInteger(row[nextIndex + i * 2], ExcelTableNames.Points);
                int end = GetValueAndCheckInteger(row[nextIndex + i * 2 + 1], ExcelTableNames.Points, x => x >= start);

                if (start > end)
                {
                    throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, ExcelTableNames.Points));
                }

                if (start < end)
                {
                    locationTimeWindows.Add(new LocationTimeWindowDto
                    {
                        PointId = id,
                        WindowStart = start < minTime ? null : TimeSpan.FromSeconds(start),
                        WindowEnd = end > maxTime ? null : TimeSpan.FromSeconds(end)
                    });
                }
            }

            nextIndex += timeWindowsCount * 2;

            locations.Add(new LocationDto
            {
                Id = id,
                PointTypeId = pointType,
                Latitude = GetValueAndCheckDouble(row[1], ExcelTableNames.Points, x => x >= minLatitude && x <= maxLatitude),
                Longitude = GetValueAndCheckDouble(row[2], ExcelTableNames.Points, x => x >= minLongitude && x <= maxLongitude),
                ServiceTime = TimeSpan.FromSeconds(GetValueAndCheckDouble(row[nextIndex], ExcelTableNames.Points, x => x >= 0)),
                LatePenalty = GetValueAndCheckInteger(row[nextIndex + 1], ExcelTableNames.Points, x => x >= 0),
                WaitPenalty = GetValueAndCheckInteger(row[nextIndex + 2], ExcelTableNames.Points, x => x >= 0)
            });
        }

        return (locations, locationDemands, locationTimeWindows);
    }

    private int GetTimeWindowsCount(DataSet ds)
    {
        var table = GetTableAndCheckExists(ds, ExcelTableNames.TimeWindows);

        int timeWindowsCount = GetValueAndCheckInteger(table.Rows[0][0], ExcelTableNames.TimeWindows, x => x > 0);

        return timeWindowsCount;
    }

    private List<DemandTypeDto> GetDemandTypeDtos(DataSet ds)
    {
        var table = GetTableAndCheckExists(ds, ExcelTableNames.DemandSize);

        int demandSize = GetValueAndCheckInteger(table.Rows[0][0], ExcelTableNames.DemandSize, x => x > 0);

        List<DemandTypeDto> demandTypes = [];

        for (int i = 1; i <= demandSize; i++)
        {
            demandTypes.Add(new DemandTypeDto
            {
                Id = i,
                Name = $"DemandType {i}"
            });
        }

        return demandTypes;
    }

    private double GetValueAndCheckDouble(object? value, string table, Predicate<double>? predicate = null)
    {
        double doubleValue = default;

        if (value is null || !double.TryParse(value.ToString(), out doubleValue))
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, table));
        }

        if (predicate != null && !predicate(doubleValue))
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, table));
        }

        return doubleValue;
    }

    private int GetValueAndCheckInteger(object? value, string table, Predicate<int>? predicate = null)
    {
        int intValue = default;

        if (value is null || !int.TryParse(value.ToString(), out intValue))
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, table));
        }

        if (predicate != null && !predicate(intValue))
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, table));
        }

        return intValue;
    }

    private DataTable GetTableAndCheckExists(DataSet ds, string tableName)
    {
        var table = ds.Tables[tableName];

        if (table == null || table.Rows.Count == 0 || table.Columns.Count == 0)
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.ExcelTableNotFound, tableName));
        }

        return table;
    }

    private bool IsExcel(Stream stream)
    {
        if (stream == null)
            return false;

        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            stream = ms;
        }

        long originalPosition = stream.Position;

        byte[] header = new byte[8];
        int bytesRead = stream.Read(header, 0, header.Length);

        stream.Position = originalPosition;

        if (bytesRead < 4)
            return false;

        // Сигнатура старого .xls (BIFF Compound File)
        byte[] xlsHeader = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        if (bytesRead >= 8 && header.Take(8).SequenceEqual(xlsHeader))
            return true;

        // Сигнатура ZIP-контейнера для .xlsx
        byte[] zipHeader = { 0x50, 0x4B, 0x03, 0x04 };
        if (header.Take(4).SequenceEqual(zipHeader))
            return true;

        return false;
    }
}
