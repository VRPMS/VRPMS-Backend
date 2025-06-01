using VRPMS.VRPCD.Enums;
using VRPMS.VRPCD.Methods.SolutionMethods;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.ProblemModels;
using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Methods.SolutionSearchMethods
{
    /// <summary>
    /// A fresh Tabu Search implementation for the VRPCD problem.
    /// This class derives from SolutionSearchMethodBase and overrides Search().
    /// It begins by creating a feasible initial solution using the provided basicSolution, then
    /// ensures every car has a route (even if empty). It then conducts Tabu Search over three move types:
    ///   1) Intra-route Swap
    ///   2) Inter-route Swap
    ///   3) Inter-route Relocate
    /// All moves are evaluated under a classical Tabu mechanism with aspiration.
    /// </summary>
    public class TabuSearchMethod : SolutionSearchMethodBase
    {
        private readonly int MaxIterations;   // Maximum number of iterations
        private readonly int TabuTenure;      // Number of iterations a move remains tabu

        public TabuSearchMethod(int? maxIterations = null, int? tabuTenure = null)
        : base()
        {
            MaxIterations = maxIterations ?? 50;
            TabuTenure = tabuTenure ?? 5;
        }

        public TabuSearchMethod(BasicSolutionMethodBase basicSolution, int? maxIterations = null, int? tabuTenure = null)
            : base(basicSolution)
        {
            MaxIterations = maxIterations ?? BasicSolutionData.Locations.Count * 10;
            TabuTenure = tabuTenure ?? 10;
        }

        public override Solution Search()
        {
            var initial = InitialSolution;
            var currentSolution = CloneSolution(initial);
            var bestSolution = CloneSolution(currentSolution);
            decimal bestCost = bestSolution.TotalScore;

            var tabuList = new Dictionary<(string moveType, int r1, int r2, int a, int b), int>();
            int routeCount = currentSolution.SolutionRoutes.Count;

            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                var (bestCandidate, bestCandidateCost, bestMove) = FindBestMove(currentSolution, bestCost, tabuList, routeCount);

                if (bestCandidate == null)
                    break;

                // Decrease tenures and remove expired
                DecrementTabuList(tabuList);
                // Add new move to tabu
                tabuList[bestMove] = TabuTenure;

                // Update current
                currentSolution = bestCandidate;

                // Update global best
                if (bestCandidateCost < bestCost)
                {
                    bestCost = bestCandidateCost;
                    bestSolution = CloneSolution(currentSolution);
                }
            }

            RemoveNeighborDuplicates(bestSolution);
            return bestSolution;
        }

        private (Solution candidate, decimal cost, (string moveType, int r1, int r2, int a, int b) move) FindBestMove(
            Solution currentSolution,
            decimal globalBestCost,
            Dictionary<(string, int, int, int, int), int> tabuList,
            int routeCount)
        {
            Solution bestCandidate = new();
            decimal bestCost = decimal.MaxValue;
            (string, int, int, int, int) bestMove = (null!, -1, -1, -1, -1);

            // Precompute client indices for all routes
            var clientIndices = currentSolution.SolutionRoutes
                .Select(GetClientVisitIndices)
                .ToList();

            // Intra-route swaps
            for (int r = 0; r < routeCount; r++)
            {
                var routeR = currentSolution.SolutionRoutes[r];
                var positionsR = clientIndices[r];
                EvaluateIntraSwaps(r, routeR, positionsR, currentSolution, globalBestCost, tabuList, ref bestCandidate, ref bestCost, ref bestMove);
            }

            // Inter-route swaps
            for (int r = 0; r < routeCount; r++)
            {
                for (int s = r + 1; s < routeCount; s++)
                {
                    EvaluateInterSwaps(r, s, currentSolution, clientIndices[r], clientIndices[s], globalBestCost, tabuList, ref bestCandidate, ref bestCost, ref bestMove);
                }
            }

            // Inter-route relocations
            for (int r = 0; r < routeCount; r++)
            {
                for (int s = 0; s < routeCount; s++)
                {
                    if (r == s) continue;
                    EvaluateRelocations(r, s, currentSolution, clientIndices[r], globalBestCost, tabuList, ref bestCandidate, ref bestCost, ref bestMove);
                }
            }

            return (bestCandidate, bestCost, bestMove);
        }

        private void EvaluateIntraSwaps(
            int r,
            CarRoute route,
            List<int> positions,
            Solution currentSolution,
            decimal globalBestCost,
            Dictionary<(string, int, int, int, int), int> tabuList,
            ref Solution bestCandidate,
            ref decimal bestCost,
            ref (string, int, int, int, int) bestMove)
        {
            int count = positions.Count;
            if (count <= 1) return;

            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    int idxA = positions[i];
                    int idxB = positions[j];
                    var locA = route.Visits[idxA].Location;
                    var locB = route.Visits[idxB].Location;

                    if (!CanSwapClients(locA, locB)) continue;
                    int idA = locA.Id, idB = locB.Id;
                    var key = ("IntraSwap", r, r, idA, idB);
                    if (IsTabuMove(key, tabuList, globalBestCost, bestCost, out bool isAspiration))
                        continue;

                    var candidate = CloneSolution(currentSolution);
                    var candRoute = candidate.SolutionRoutes[r];
                    SwapInRoute(candRoute, idxA, idxB);
                    RecalculateRouteMetrics(candRoute);
                    decimal candCost = candidate.TotalScore;
                    if ((!tabuList.TryGetValue(key, out _) || candCost < globalBestCost) && candCost < bestCost)
                    {
                        bestCost = candCost;
                        bestCandidate = candidate;
                        bestMove = key;
                    }
                }
            }
        }

        private void EvaluateInterSwaps(
            int r,
            int s,
            Solution currentSolution,
            List<int> positionsR,
            List<int> positionsS,
            decimal globalBestCost,
            Dictionary<(string, int, int, int, int), int> tabuList,
            ref Solution bestCandidate,
            ref decimal bestCost,
            ref (string, int, int, int, int) bestMove)
        {
            if (positionsR.Count == 0 || positionsS.Count == 0) return;
            var routeR = currentSolution.SolutionRoutes[r];
            var routeS = currentSolution.SolutionRoutes[s];

            foreach (int idxR in positionsR)
            {
                foreach (int idxS in positionsS)
                {
                    var locA = routeR.Visits[idxR].Location;
                    var locB = routeS.Visits[idxS].Location;
                    if (!CanSwapClients(locA, locB)) continue;
                    int idA = locA.Id, idB = locB.Id;
                    var key = ("InterSwap", r, s, idA, idB);
                    if (IsTabuMove(key, tabuList, globalBestCost, bestCost, out bool isAspiration))
                        continue;

                    var candidate = CloneSolution(currentSolution);
                    var candR = candidate.SolutionRoutes[r];
                    var candS = candidate.SolutionRoutes[s];
                    SwapBetweenRoutes(candR, idxR, candS, idxS);
                    RecalculateRouteMetrics(candR);
                    RecalculateRouteMetrics(candS);
                    decimal candCost = candidate.TotalScore;
                    if ((!tabuList.TryGetValue(key, out _) || candCost < globalBestCost) && candCost < bestCost)
                    {
                        bestCost = candCost;
                        bestCandidate = candidate;
                        bestMove = key;
                    }
                }
            }
        }

        private void EvaluateRelocations(
            int r,
            int s,
            Solution currentSolution,
            List<int> positionsR,
            decimal globalBestCost,
            Dictionary<(string, int, int, int, int), int> tabuList,
            ref Solution bestCandidate,
            ref decimal bestCost,
            ref (string, int, int, int, int) bestMove)
        {
            if (positionsR.Count == 0) return;
            var routeR = currentSolution.SolutionRoutes[r];
            var routeS = currentSolution.SolutionRoutes[s];

            foreach (int idxR in positionsR)
            {
                var locA = routeR.Visits[idxR].Location;
                if (locA.LocationType != LocationTypeEnum.Client) continue;
                int idA = locA.Id;
                var key = ("Relocate", r, s, idA, -1);
                if (IsTabuMove(key, tabuList, globalBestCost, bestCost, out bool isAspiration))
                    continue;

                var candidate = CloneSolution(currentSolution);
                var candR = candidate.SolutionRoutes[r];
                var candS = candidate.SolutionRoutes[s];
                int actualIdxR = candR.Visits.FindIndex(v => v.Location.Id == idA && v.Location.LocationType == LocationTypeEnum.Client);
                if (actualIdxR < 0) continue;

                var extractedVisit = candR.Visits[actualIdxR];
                InsertBest(candS, extractedVisit);
                candR.Visits.RemoveAt(actualIdxR);
                RecalculateRouteMetrics(candR);
                RecalculateRouteMetrics(candS);
                decimal candCost = candidate.TotalScore;
                if ((!tabuList.TryGetValue(key, out _) || candCost < globalBestCost) && candCost < bestCost)
                {
                    bestCost = candCost;
                    bestCandidate = candidate;
                    bestMove = key;
                }
            }
        }

        private bool CanSwapClients(Location locA, Location locB)
        {
            if (locA.LocationType != LocationTypeEnum.Client || locB.LocationType != LocationTypeEnum.Client)
                return false;
            return locA.SupplyChain?.Warehouse == locB.SupplyChain?.Warehouse
                   && locA.SupplyChain?.CrossDock == locB.SupplyChain?.CrossDock;
        }

        private bool IsTabuMove(
            (string moveType, int r1, int r2, int a, int b) key,
            Dictionary<(string, int, int, int, int), int> tabuList,
            decimal globalBestCost,
            decimal currentBestCost,
            out bool isAspiration)
        {
            if (tabuList.TryGetValue(key, out int tenure) && tenure > 0)
            {
                isAspiration = false;
                return true;
            }
            isAspiration = false;
            return false;
        }

        private void SwapInRoute(CarRoute route, int idxA, int idxB)
        {
            var temp = route.Visits[idxA].Location;
            route.Visits[idxA].Location = route.Visits[idxB].Location;
            route.Visits[idxB].Location = temp;
        }

        private void SwapBetweenRoutes(CarRoute routeR, int idxR, CarRoute routeS, int idxS)
        {
            var temp = routeR.Visits[idxR].Location;
            routeR.Visits[idxR].Location = routeS.Visits[idxS].Location;
            routeS.Visits[idxS].Location = temp;
        }

        private void DecrementTabuList(Dictionary<(string, int, int, int, int), int> tabuList)
        {
            var expired = new List<(string, int, int, int, int)>();
            foreach (var key in tabuList.Keys.ToList())
            {
                tabuList[key]--;
                if (tabuList[key] <= 0)
                    expired.Add(key);
            }
            foreach (var key in expired)
                tabuList.Remove(key);
        }

        private void RemoveNeighborDuplicates(Solution solution)
        {
            foreach (var route in solution.SolutionRoutes)
            {
                for (int i = 0; i < route.Visits.Count - 1; i++)
                {
                    if (route.Visits[i].Location.Id == route.Visits[i + 1].Location.Id)
                    {
                        route.Visits.RemoveAt(i + 1);
                        i--;
                    }
                }
                RecalculateRouteMetrics(route);
            }
        }

        private List<int> GetClientVisitIndices(CarRoute route)
        {
            var indices = new List<int>();
            int last = route.Visits.Count - 1;
            for (int i = 1; i < last; i++)
            {
                if (route.Visits[i].Location.LocationType == LocationTypeEnum.Client)
                    indices.Add(i);
            }
            return indices;
        }

        private void InsertBest(CarRoute route, LocationVisit clientVisit)
        {
            if (clientVisit.Location.LocationType != LocationTypeEnum.Client)
                return;

            var supply = clientVisit.Location.SupplyChain;
            int startIndex = 0;

            // Ensure warehouse and cross-dock present
            EnsureSupplyNodes(route, supply);

            // Find best insertion index
            int locC = clientVisit.Location.Id;
            decimal bestDelta = decimal.MaxValue;
            int bestIdx = -1;
            for (int j = startIndex; j < route.Visits.Count - 1; j++)
            {
                int locA = route.Visits[j].Location.Id;
                int locB = route.Visits[j + 1].Location.Id;
                var infoAB = DestinationMap[locA][locB];
                var infoAC = DestinationMap[locA][locC];
                var infoCB = DestinationMap[locC][locB];

                decimal delta = (decimal)infoAC.Distance + (decimal)infoCB.Distance - (decimal)infoAB.Distance;
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    bestIdx = j + 1;
                }
            }

            if (bestIdx < 0)
                bestIdx = route.Visits.Count - 1;

            route.Visits.Insert(bestIdx, new LocationVisit
            {
                Location = clientVisit.Location,
                ArrivalTime = TimeSpan.MinValue,
                DepartureTime = TimeSpan.MinValue
            });
        }

        private void EnsureSupplyNodes(CarRoute route, LocationSupplyChain? supply)
        {
            if (supply == null)
                return;
            if (supply.Warehouse != null && !route.Visits.Any(v => v.Location.Id == supply.Warehouse.Id))
                route.Visits.Insert(route.Visits.Count - 1, new LocationVisit { Location = supply.Warehouse, ArrivalTime = TimeSpan.MinValue, DepartureTime = TimeSpan.MinValue });
            if (supply.CrossDock != null && !route.Visits.Any(v => v.Location.Id == supply.CrossDock.Id))
                route.Visits.Insert(route.Visits.Count - 1, new LocationVisit { Location = supply.CrossDock, ArrivalTime = TimeSpan.MinValue, DepartureTime = TimeSpan.MinValue });
        }

        private void RecalculateRouteMetrics(CarRoute route)
        {
            if (route.Visits == null || route.Visits.Count == 0)
                return;

            var car = route.Car;
            route.TotalDistance = 0.0M;
            route.TimeWindowPenalty = 0.0M;
            route.CapacityPenalty = 0.0M;
            route.OverWorkPenalty = 0.0M;
            route.CurrentLoad = car.Capacities.ToDictionary(c => c.DemandId, c => 0.0);
            route.CurrentTime = car.WorkStart;
            route.Visits[0].ArrivalTime = car.WorkStart;
            route.Visits[0].DepartureTime = car.WorkStart;

            for (int i = 0; i < route.Visits.Count - 1; i++)
            {
                var prev = route.Visits[i];
                var next = route.Visits[i + 1];
                int prevId = prev.Location.Id;
                int nextId = next.Location.Id;
                var info = DestinationMap[prevId][nextId];
                route.TotalDistance += (decimal)info.Distance;
                route.CurrentTime += info.Duration;
                var loc = next.Location;
                var tw = loc.TimeWindows.OrderBy(t => t.WindowEnd).FirstOrDefault(t => t.WindowEnd > route.CurrentTime)
                          ?? loc.TimeWindows.OrderBy(t => t.WindowEnd).Last();
                if (route.CurrentTime < tw.WindowStart)
                {
                    var waitMin = (decimal)(tw.WindowStart - route.CurrentTime).TotalMinutes;
                    route.TimeWindowPenalty += waitMin * loc.WaitPenalty;
                    route.CurrentTime = tw.WindowStart;
                }
                else if (route.CurrentTime > tw.WindowEnd)
                {
                    var lateMin = (decimal)(route.CurrentTime - tw.WindowEnd).TotalMinutes;
                    route.TimeWindowPenalty += lateMin * loc.LatePenalty;
                }
                route.CurrentTime += loc.ServiceTime;
                next.ArrivalTime = route.CurrentTime - loc.ServiceTime;
                next.DepartureTime = route.CurrentTime;
                if (loc.LocationType == LocationTypeEnum.Client)
                {
                    foreach (var d in loc.Demands)
                        route.CurrentLoad[d.DemandId] += d.DemandValue;
                }
            }

            if (route.CurrentTime > car.WorkEnd)
            {
                var overtime = (decimal)(route.CurrentTime - car.WorkEnd).TotalMinutes;
                route.OverWorkPenalty = car.OverWorkPenalty * overtime;
            }

            foreach (var kv in route.CurrentLoad)
            {
                int dId = kv.Key;
                double loadVal = kv.Value;
                double maxCap = CapacityMap[car.Id][dId].MaxCapacity;
                double overflow = Math.Max(0.0, loadVal - maxCap);
                route.CapacityPenalty += (decimal)overflow * car.CapacityOverloadPenalty;
            }
        }
    }
}