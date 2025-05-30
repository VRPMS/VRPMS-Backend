using VRPMS.VRPCD.Models.ProblemModels;

namespace VRPMS.VRPCD.Models;

public class Problem
{
    public List<Location> Locations { get; set; } = [];
     
    public List<Car> Cars { get; set; } = [];
}
