using VRPMS.VRPCD.Methods.SolutionMethods;
using VRPMS.VRPCD.Models;
using VRPMS.VRPCD.Models.ProblemModels;
using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Methods.SolutionSearchMethods;

public abstract class SolutionSearchMethodBase
{
    private BasicSolutionMethodBase basicSolutionData = null!;

    public SolutionSearchMethodBase() { }

    public SolutionSearchMethodBase(BasicSolutionMethodBase basicSolution)
    {
        InitProperties(basicSolution);
    }

    private void InitProperties(BasicSolutionMethodBase basicSolution)
    {
        basicSolutionData = basicSolution ?? throw new ArgumentNullException(nameof(basicSolution));
        Problem = basicSolution.Problem ?? throw new ArgumentNullException(nameof(basicSolution.Problem));
        InitialSolution = CloneSolution(basicSolution.Solution ?? throw new ArgumentNullException(nameof(basicSolution.Solution)));
        DestinationMap = basicSolution.DestinationMap ?? throw new ArgumentNullException(nameof(basicSolution.DestinationMap));
        CapacityMap = basicSolution.CapacityMap ?? throw new ArgumentNullException(nameof(basicSolution.CapacityMap));
    }

    public abstract Solution Search();

    public BasicSolutionMethodBase BasicSolutionData { get => basicSolutionData; set => InitProperties(value); }

    public Problem Problem { get; set; } = null!;

    public Solution InitialSolution { get; set; } = null!;
    
    public Dictionary<int, Dictionary<int, LocationDestination>> DestinationMap { get; protected set; } = [];

    public Dictionary<int, Dictionary<int, CarCapacity>> CapacityMap { get; protected set; } = [];

    protected Solution CloneSolution(Solution original)
    {
        var newSolution = new Solution();

        foreach (var route in original.SolutionRoutes)
        {
            newSolution.SolutionRoutes.Add(CloneRouteStructure(route));
        }

        return newSolution;
    }

    private CarRoute CloneRouteStructure(CarRoute route)
    {
        var newRoute = new CarRoute
        {
            Car = route.Car,
            CurrentTime = route.CurrentTime,
            CapacityPenalty = route.CapacityPenalty,
            TimeWindowPenalty = route.TimeWindowPenalty,
            OverWorkPenalty = route.OverWorkPenalty,
            TotalDistance = route.TotalDistance,
            CurrentLoad = route.CurrentLoad.ToDictionary(kv => kv.Key, kv => kv.Value),
            Visits = route.Visits
                        .Select(v => new LocationVisit
                        {
                            Location = v.Location,
                            ArrivalTime = v.ArrivalTime,
                            DepartureTime = v.DepartureTime
                        })
                        .ToList()
        };

        return newRoute;
    }
}
