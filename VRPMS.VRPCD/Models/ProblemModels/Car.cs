namespace VRPMS.VRPCD.Models.ProblemModels;

public class Car
{
    public int Id { get; set; }

    public int CapacityOverloadPenalty { get; set; }

    public int MaxCapacityOverloadPenalty { get; set; }

    public TimeSpan WorkStart { get; set; }

    public TimeSpan WorkEnd { get; set; }

    public int OverWorkPenalty { get; set; }

    public Location RouteStart { get; set; } = null!;

    public List<CarCapacity> Capacities { get; set; } = [];
}
