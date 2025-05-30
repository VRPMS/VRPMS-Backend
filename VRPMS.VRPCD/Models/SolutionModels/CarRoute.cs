using VRPMS.VRPCD.Models.ProblemModels;

namespace VRPMS.VRPCD.Models.SolutionModels;

public class CarRoute
{
    public Car Car { get; set; } = null!;
   
    public List<LocationVisit> Visits { get; set; } = [];

    // Collected metrics
    public decimal TotalDistance { get; set; } = 0.0M;

    public decimal TimeWindowPenalty { get; set; } = 0.0M;

    public decimal OverWorkPenalty { get; set; } = 0.0M;
    
    public decimal CapacityPenalty { get; set; } = 0.0M;

    // Helper properties for current state
    public TimeSpan CurrentTime { get; set; }
    
    public Dictionary<int, double> CurrentLoad { get; set; } = [];
}
