using VRPMS.DataAccess.Interfaces.Constants;
using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.VRPCD.Enums;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.ProblemModels;

namespace VRPMS.BusinessLogic.Helpers;

public static class VrpcdHelper
{
    public static Problem GetProblem(DataDto dataDto)
    {
        Problem problem = new Problem
        {
            Locations = dataDto.Locations.Select(location => new Location
            {
                Id = location.Id,
                LocationType = (LocationTypeEnum)location.LocationTypeId,
                ServiceTime = location.ServiceTime,
                LatePenalty = location.LatePenalty,
                WaitPenalty = location.WaitPenalty,

                Demands = dataDto.LocationDemands
                    .Where(demand => demand.LocationId == location.Id)
                    .Select(demand => new LocationDemand
                    {
                        DemandId = demand.DemandId,
                        DemandValue = demand.DemandValue
                    }).ToList(),

                TimeWindows = dataDto.LocationTimeWindows
                    .Where(timeWindow => timeWindow.LocationId == location.Id)
                    .Select(timeWindow => new LocationTimeWindow
                    {
                        WindowStart = timeWindow.WindowStart ?? TimeConstants.DefaultWindowStart,
                        WindowEnd = timeWindow.WindowEnd ?? TimeConstants.DefaultWindowEnd
                    }).ToList(),

            }).ToList(),
        };

        problem.Cars = dataDto.Cars.Select(car => new Car
        {
            Id = car.Id,
            CapacityOverloadPenalty = car.CapacityOverloadPenalty,
            MaxCapacityOverloadPenalty = car.MaxCapacityOverloadPenalty,
            WorkStart = car.WorkStart ?? TimeConstants.DefaultWindowStart,
            WorkEnd = car.WorkEnd ?? TimeConstants.DefaultWindowEnd,
            OverWorkPenalty = car.OverWorkPenalty,
            RouteStart = problem.Locations.First(x => x.Id == car.RouteStartLocationId),

            Capacities = dataDto.CarCapacities
                .Where(capacity => capacity.CarId == car.Id)
                .Select(capacity => new CarCapacity
                {
                    DemandId = capacity.DemandId,
                    Capacity = capacity.Capacity,
                    MaxCapacity = capacity.MaxCapacity
                }).ToList(),

        }).ToList();

        foreach (var location in problem.Locations)
        {
            location.Destinations = dataDto.LocationRoutes
                .Where(route => route.FromPointId == location.Id)
                .Select(route => new LocationDestination
                {
                    DestinationLocation = problem.Locations.First(l => l.Id == route.ToPointId),
                    Distance = route.Distance,
                    Duration = route.Duration
                }).ToList();

            if (location.LocationType is LocationTypeEnum.Client)
            {
                location.SupplyChain = dataDto.LocationSupplyChains
                    .Where(supplyChain => supplyChain.ClientId == location.Id)
                    .Select(supplyChain => new LocationSupplyChain
                    {
                        Warehouse = problem.Locations.FirstOrDefault(l => l.Id == supplyChain.WarehouseId),
                        CrossDock = problem.Locations.FirstOrDefault(l => l.Id == supplyChain.CrossDockId)
                    })
                    .First();
            }
        }

        return problem;
    }

    public static SolutionDto GetSolutionDto(Solution solution)
    {
        return new SolutionDto
        {
            TotalScore = solution.TotalScore,
            SolutionRoutes = solution.SolutionRoutes.Select(route => new SolutionRouteDto
            {
                CarId = route.Car.Id,
                SolutionRouteVisits = route.Visits.Select((visit, index) => new SolutionRouteVisitDto
                {
                    SequenceNumber = index + 1,
                    LocationId = visit.Location.Id,
                    ArrivalTime = visit.ArrivalTime,
                    DepartureTime = visit.DepartureTime
                }).ToList()

            }).ToList()
        };
    }
}
