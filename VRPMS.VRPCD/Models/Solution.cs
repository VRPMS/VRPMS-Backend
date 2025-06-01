using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Models;

public class Solution
{
    public List<CarRoute> SolutionRoutes { get; set; } = [];

    public decimal TotalScore
    {
        get
        {
            return SolutionRoutes.Sum(r =>
                r.TotalDistance
                + r.CapacityPenalty
                + r.TimeWindowPenalty
                + r.OverWorkPenalty
                + (decimal)(r.Visits.LastOrDefault()?.ArrivalTime.TotalSeconds ?? 0)
                - (decimal)(r.Visits.FirstOrDefault()?.ArrivalTime.TotalSeconds ?? 0));
        }
    }
}
