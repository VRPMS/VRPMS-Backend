using VRPMS.VRPCD.Enums;
using VRPMS.VRPCD.Helpers;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.ProblemModels;
using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Methods.SolutionMethods;

public abstract class BasicSolutionMethodBase
{
    public BasicSolutionMethodBase() {}

    public BasicSolutionMethodBase(Problem problem)
    {
        InitProperties(problem);
    }

    private void InitProperties(Problem? problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem), ErrorMessages.ProblemCannotBeNull);
        }

        this.problem = problem;

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

    protected record Job(int[] SupplyNodeIds, HashSet<int> ClientIds);

    protected virtual List<Job> BuildJobs(HashSet<int> remainingClientIds)
    {
        var jobs = new List<Job>();

        foreach (var warehouse in Warehouses)
        {
            // DIRECT: Warehouse → Client
            var direct = remainingClientIds
                    .Where(clientId => Clients[clientId].SupplyChain?.Warehouse?.Id == warehouse.Key
                                    && Clients[clientId].SupplyChain?.CrossDock == null)
                    .ToHashSet();

            if (direct.Any())
            {
                jobs.Add(new Job([warehouse.Key], direct));
            }

            // VIA: Warehouse → CrossDock → Client
            var viaGroups = remainingClientIds
                    .Select(clientId => Clients[clientId])
                    .Where(client => client.SupplyChain?.Warehouse?.Id == warehouse.Key
                                  && client.SupplyChain?.CrossDock != null)
                    .GroupBy(c => c.SupplyChain!.CrossDock!.Id);

            foreach (var group in viaGroups)
            {
                jobs.Add(new Job([warehouse.Key, group.Key],
                    group.Select(client => client.Id).ToHashSet()));
            }
        }

        // ATDOCK: CrossDock → Client
        foreach (var crossDock in CrossDocks)
        {
            var atDock = remainingClientIds
                    .Where(clientId => Clients[clientId].SupplyChain?.Warehouse == null
                               && Clients[clientId].SupplyChain?.CrossDock?.Id == crossDock.Key)
                    .ToHashSet();

            if (atDock.Any())
            {
                jobs.Add(new Job([crossDock.Key], atDock));
            }
        }

        return jobs;
    }
    
    protected virtual int VisitLocation(
        CarRoute route,
        int previousLocationId,
        int nextLocationId)
    {
        // 1) travel time
        route.TotalDistance += (decimal)DestinationMap[previousLocationId][nextLocationId].Distance;
        var travel = DestinationMap[previousLocationId][nextLocationId].Duration;
        route.CurrentTime += travel;

        // 2) Arrival
        var arrival = route.CurrentTime;
        var nextLocation = Locations[nextLocationId];

        // 3) Time Windows
        var timeWindow = nextLocation.TimeWindows
            .OrderBy(tw => tw.WindowEnd)
            .FirstOrDefault(tw => tw.WindowEnd > arrival)
            ?? nextLocation.TimeWindows
            .OrderBy(tw => tw.WindowEnd)
            .Last();

        var windowStart = timeWindow.WindowStart;
        var windowEnd = timeWindow.WindowEnd;

        if (arrival < windowStart)
        {
            var wait = (decimal)(windowStart - arrival).TotalMinutes;
            route.TimeWindowPenalty += wait * nextLocation.WaitPenalty;
            arrival = windowStart;
        }
        else if (arrival > windowEnd)
        {
            var late = (decimal)(arrival - windowEnd).TotalMinutes;
            route.TimeWindowPenalty += late * nextLocation.LatePenalty;
        }

        // 4) service time
        route.CurrentTime = arrival + nextLocation.ServiceTime;

        route.Visits.Add(new LocationVisit
        {
            Location = nextLocation,
            ArrivalTime = arrival,
            DepartureTime = route.CurrentTime
        });

        return nextLocationId;
    }

    public abstract Solution Solve(Problem? problem = null);

    private Problem? problem;

    public Problem? Problem { get => problem; set => InitProperties(value); }

    public Dictionary<int, Car> Cars { get; protected set; } = [];

    public Dictionary<int, Location> Locations { get; protected set; } = [];

    public Dictionary<int, Location> Warehouses { get; protected set; } = [];

    public Dictionary<int, Location> CrossDocks { get; protected set; } = [];

    public Dictionary<int, Location> Clients { get; protected set; } = [];

    public Dictionary<int, Dictionary<int, LocationDestination>> DestinationMap { get; protected set; } = [];

    public Dictionary<int, Dictionary<int, CarCapacity>> CapacityMap { get; protected set; } = [];

    public Solution Solution { get; set; } = new();
}
