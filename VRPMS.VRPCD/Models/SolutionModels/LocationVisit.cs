using VRPMS.VRPCD.Models.ProblemModels;

namespace VRPMS.VRPCD.Models.SolutionModels;

public class LocationVisit
{
    public required Location Location { get; set; } = null!;

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan? DepartureTime { get; set; }
}
