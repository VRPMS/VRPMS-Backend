using VRPMS.VRPCD.Models.SolutionModels;

namespace VRPMS.VRPCD.Models;

public class Solution
{
    public List<CarRoute> SolutionRoutes { get; set; } = [];

    public decimal TotalScore { get; set; } = 0.0M;
}
