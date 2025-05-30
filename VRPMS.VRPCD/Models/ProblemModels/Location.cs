using VRPMS.VRPCD.Enums;

namespace VRPMS.VRPCD.Models.ProblemModels;

public class Location
{
    public int Id { get; set; }

    public LocationTypeEnum LocationType { get; set; }

    public TimeSpan ServiceTime { get; set; }

    public int LatePenalty { get; set; }

    public int WaitPenalty { get; set; }

    public List<LocationDemand> Demands { get; set; } = [];

    public List<LocationTimeWindow> TimeWindows { get; set; } = [];

    public List<LocationDestination> Destinations { get; set; } = [];
   
    public LocationSupplyChain SupplyChain { get; set; } = new();
}
