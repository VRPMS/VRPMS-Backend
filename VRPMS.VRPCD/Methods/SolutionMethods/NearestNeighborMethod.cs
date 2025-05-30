using VRPMS.VRPCD.Methods.SolutionMethods;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Methods.BasicSolutionMethods;

public class NearestNeighborMethod : SolutionMethodBase
{
    private record Job(int[] SupplyNodeIds, HashSet<int> ClientIds);

    public override Solution Solve(Problem problem)
    {
        InitProperties(problem);

        List<int> remainingCars = Cars.Keys.ToList();
        HashSet<int> remainingClients = Clients.Keys.ToHashSet();

        // Until we have Clients
        while (remainingClients.Any())
        {
            // 1) Forming actual list of jobs for cars
            List<Job> jobs = BuildJobs(remainingClients);

            if (!jobs.Any())
            {
                break; // Nothing to do, no jobs available, all clients served
            }

            // If there are no remaining cars, but there are still unvisited clients - refresh the list of cars
            if (!remainingCars.Any())
            {
                remainingCars.AddRange(Cars.Keys.ToList());
            }

            // 2) Looking best car for the job in terms of minimal distance for first supply nodeId
            var best = (
                    from job in jobs
                    from carId in remainingCars
                    let startId = Cars[carId].RouteStart.Id
                    let firstId = job.SupplyNodeIds[0]
                    let d = DestinationMap[startId][firstId].Distance
                    select new { job, carId, d }
                ).MinBy(x => x.d)!;

            // 3) Building route for selected route and supplier
            var removedIds = BuildRouteForJob(best.carId, best.job);

            // 4) Refreshing Unvisited ClientIds and remaining cars
            foreach (var clientId in removedIds)
            {
                remainingClients.Remove(clientId);
            }

            remainingCars.Remove(best.carId);
        }

        foreach (var route in Solution.SolutionRoutes)
        {
            // 5) Returning cars to start location
            var lastLocationId = route.Visits.Last().Location.Id;
            var depo = route.Car.RouteStart;
            
            route.TotalDistance += (decimal)DestinationMap[lastLocationId][depo.Id].Distance;
            route.CurrentTime = route.CurrentTime + DestinationMap[lastLocationId][depo.Id].Duration;

            route.Visits.Add(new LocationVisit
            {
                Location = depo,
                ArrivalTime = route.CurrentTime
            });

            // 6) Calculating overwork penalties for each route
            var workEnd = route.Car.WorkEnd;

            if (route.CurrentTime > workEnd)
            {
                route.OverWorkPenalty = route.Car.OverWorkPenalty * (decimal)(route.CurrentTime - workEnd).TotalMinutes;
            }
        }

        Solution.TotalScore = GetTotalScore();
        return Solution;
    }

    private List<Job> BuildJobs(HashSet<int> remainingClientIds)
    {
        var jobs = new List<Job>();

        foreach (var warehouse in Warehouses)
        {
            // DIRECT: Warehouse → Client
            var direct = remainingClientIds
                    .Where(clientId => Clients[clientId].SupplyChain.Warehouse?.Id == warehouse.Key
                                    && Clients[clientId].SupplyChain.CrossDock == null)
                    .ToHashSet();

            if (direct.Any())
            {
                jobs.Add(new Job([warehouse.Key], direct));
            }

            // VIA: Warehouse → CrossDock → Client
            var viaGroups = remainingClientIds
                    .Select(clientId => Clients[clientId])
                    .Where(client => client.SupplyChain.Warehouse?.Id == warehouse.Key
                                  && client.SupplyChain.CrossDock != null)
                    .GroupBy(c => c.SupplyChain.CrossDock!.Id);

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
                    .Where(clientId => Clients[clientId].SupplyChain.Warehouse == null
                               && Clients[clientId].SupplyChain.CrossDock?.Id == crossDock.Key)
                    .ToHashSet();

            if (atDock.Any())
            {
                jobs.Add(new Job([crossDock.Key], atDock));
            }
        }

        return jobs;
    }

    private List<int> BuildRouteForJob(int carId, Job job)
    {
        var car = Cars[carId];

        // 1) Find existing route for car or create new one
        var route =
            Solution.SolutionRoutes.FirstOrDefault(r => r.Car == car)
            ?? new CarRoute
            {
                Car = car,
                CurrentLoad = car.Capacities.ToDictionary(c => c.DemandId, c => 0.0),
                CapacityPenalty = 0.0M,
                TimeWindowPenalty = 0.0M,
                OverWorkPenalty = 0.0M,
                CurrentTime = car.WorkStart
            };

        if (!Solution.SolutionRoutes.Contains(route))
        {
            Solution.SolutionRoutes.Add(route);

            route.Visits.Add(new LocationVisit
            {
                Location = car.RouteStart,
                ArrivalTime = route.CurrentTime,
                DepartureTime = route.CurrentTime
            });
        }
        else
        {
            // Resetting currentLocationId load for existing route
            route.CurrentLoad = car.Capacities.ToDictionary(c => c.DemandId, c => 0.0);
        }

        // 2) Move to Supply Nodes
        var currentLocationId = route.Visits.Last().Location.Id;

        foreach (var nodeId in job.SupplyNodeIds)
        {
            if (nodeId != currentLocationId)
            {
                // problem
                currentLocationId = VisitLocation(route, currentLocationId, nodeId);
            }
        }

        // 3) Serve ClientIds by Nearest Neighbor logic
        return ServeNearest(route, currentLocationId, job.ClientIds);
    }

    private List<int> ServeNearest(
        CarRoute route,
        int routeStartId,
        HashSet<int> toServeIds)
    {
        List<int> removedLocationIds = [];
        var currentLocationId = routeStartId;

        while (toServeIds.Any())
        {
            var nextLocationId = toServeIds.MinBy(destinationId => DestinationMap[currentLocationId][destinationId].Distance)!;
            var nextLocation = Clients[nextLocationId];

            // If Car is not able to serve nextLocationId clientId due to capacity limits, break the loop
            if (nextLocation.Demands.Any(demand => 
                route.CurrentLoad[demand.DemandId] + demand.DemandValue > 
                CapacityMap[route.Car.Id][demand.DemandId].MaxCapacity))
            {
                break;
            }

            currentLocationId = VisitLocation(route, currentLocationId, nextLocationId);

            // Update currentLocationId load for route
            foreach (var demand in nextLocation.Demands)
            {
                route.CurrentLoad[demand.DemandId] += demand.DemandValue;
            }

            removedLocationIds.Add(nextLocationId);
            toServeIds.Remove(nextLocationId);
        }

        // Calculate Capacity Penalty for the route
        foreach (var load in route.CurrentLoad)
        {
            route.CapacityPenalty +=
                (decimal)Math.Max(0.0, load.Value - CapacityMap[route.Car.Id][load.Key].Capacity)
                * route.Car.CapacityOverloadPenalty;
        }

        return removedLocationIds;
    }

    private int VisitLocation(
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
}   
