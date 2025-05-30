using VRPMS.VRPCD.Enums;
using VRPMS.VRPCD.Helpers;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.ProblemModels;

namespace VRPMS.VRPCD.Methods.SolutionMethods;

public abstract class SolutionMethodBase
{
    protected void InitProperties(Problem problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem), ErrorMessages.ProblemCannotBeNull);
        }

        Cars = problem.Cars.ToDictionary(car => car.Id, car => car);

        if (!Cars.Any() || Cars.Any(x => !x.Value.Capacities.Any()))
        {
            throw new ArgumentException(ErrorMessages.CarNotFound);
        }

        Locations = problem.Locations.ToDictionary(location => location.Id, location => location);
       
        if (!Locations.Any() || Locations.Any(x => !x.Value.Destinations.Any()))
        {
            throw new ArgumentException(ErrorMessages.LocationNotFound);
        }

        Warehouses = problem.Locations
            .Where(x => x.LocationType == LocationTypeEnum.Warehouse)
            .ToDictionary(location => location.Id, location => location);

        if (!Warehouses.Any())
        {
            throw new ArgumentException(ErrorMessages.WarehouseNotFound);
        }

        CrossDocks = problem.Locations
            .Where(x => x.LocationType == LocationTypeEnum.CrossDock)
            .ToDictionary(location => location.Id, location => location);

        if (!CrossDocks.Any())
        {
            throw new ArgumentException(ErrorMessages.CrossDockNotFound);
        }

        Clients = problem.Locations
            .Where(x => x.LocationType == LocationTypeEnum.Client
                        && x.SupplyChain != null
                        && !(x.SupplyChain.CrossDock == null && x.SupplyChain.Warehouse == null)
                        && x.Demands.Any())
            .ToDictionary(location => location.Id, location => location);

        if (!Clients.Any())
        {
            throw new ArgumentException(ErrorMessages.ClientNotFound);
        }

        DestinationMap = problem.Locations.ToDictionary(
                l => l.Id,
                l => l.Destinations.ToDictionary(d => d.DestinationLocation.Id)
            );

        CapacityMap = Cars.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Capacities.ToDictionary(cc => cc.DemandId)
        );

        Solution = new();
    }

    protected decimal GetTotalScore()
    {
        return Solution.SolutionRoutes.Sum(r => r.TotalDistance + r.CapacityPenalty + r.TimeWindowPenalty + r.OverWorkPenalty);
    }

    protected Dictionary<int, Car> Cars { get; set; } = [];

    protected Dictionary<int, Location> Locations { get; set; } = [];

    protected Dictionary<int, Location> Warehouses { get; set; } = [];

    protected Dictionary<int, Location> CrossDocks { get; set; } = [];

    protected Dictionary<int, Location> Clients { get; set; } = [];

    protected Dictionary<int, Dictionary<int, LocationDestination>> DestinationMap = null!;
    
    protected Dictionary<int, Dictionary<int, CarCapacity>> CapacityMap = null!;

    protected Solution Solution { get; set; } = new();

    public abstract Solution Solve(Problem problem);
}
