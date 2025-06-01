using VRPMS.VRPCD.Helpers;
using VRPMS.VRPCD.Methods.SolutionMethods;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Methods.BasicSolutionMethods;

public class NearestNeighborMethod : BasicSolutionMethodBase
{
    public NearestNeighborMethod() { }

    public NearestNeighborMethod(Problem problem) : base(problem) { }

    public override Solution Solve(Problem? problem = null)
    {
        if (problem != null)
        {
            Problem = problem;
        }

        if (Problem == null)
        {
            throw new ArgumentNullException(nameof(problem), ErrorMessages.ProblemCannotBeNull);
        }

        List<int> remainingCars = Cars.Keys.ToList();
        HashSet<int> remainingClients = Clients.Keys.ToHashSet();
        bool extendCapacity = false;
        bool extendMaxCapacity = false;

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
                if (extendCapacity)
                {
                    extendMaxCapacity = true;
                }

                extendCapacity = true;

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
            var removedIds = BuildRouteForJob(best.carId, best.job, extendCapacity, extendMaxCapacity);

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

        if (!extendCapacity)
        {
            // 7) If we have remaining cars, we need to add them to the solution with empty routes
            foreach (var carId in remainingCars)
            {
                var car = Cars[carId];

                Solution.SolutionRoutes.Add(new CarRoute
                {
                    Car = car,
                    CurrentLoad = car.Capacities.ToDictionary(c => c.DemandId, c => 0.0),
                    CapacityPenalty = 0.0M,
                    TimeWindowPenalty = 0.0M,
                    OverWorkPenalty = 0.0M,
                    TotalDistance = 0.0M,
                    CurrentTime = car.WorkStart,
                    Visits =
                    [
                        new LocationVisit
                        {
                            Location = car.RouteStart,
                            ArrivalTime = car.WorkStart
                        }
                    ]
                });
            }
        }

        return Solution;
    }

    private List<int> BuildRouteForJob(
        int carId, 
        Job job,
        bool extendCapacity = false,
        bool extendMaxCapacity = false)
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
        return ServeNearest(route, currentLocationId, job.ClientIds, extendCapacity, extendMaxCapacity);
    }

    private List<int> ServeNearest(
        CarRoute route,
        int routeStartId,
        HashSet<int> toServeIds,
        bool extendCapacity = false,
        bool isRefreshed = false)
    {
        List<int> removedLocationIds = [];
        var currentLocationId = routeStartId;

        while (toServeIds.Any())
        {
            var nextLocationId = toServeIds.MinBy(destinationId => DestinationMap[currentLocationId][destinationId].Distance)!;
            var nextLocation = Clients[nextLocationId];

            // If Car is not able to serve nextLocationId clientId due to capacity limits, break the loop
            if (!isRefreshed)
            {
                if (nextLocation.Demands.Any(demand =>
                    route.CurrentLoad[demand.DemandId] + demand.DemandValue >
                    CapacityMap[route.Car.Id][demand.DemandId].MaxCapacity))
                {
                    break;
                }
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
}   
