namespace VRPMS.VRPCD.Models.ProblemModels;

public class LocationDestination
{
    public Location DestinationLocation { get; set; } = null!;

    public TimeSpan Duration { get; set; }

    public double Distance { get; set; }
}
