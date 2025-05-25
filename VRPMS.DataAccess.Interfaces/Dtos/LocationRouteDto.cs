namespace VRPMS.DataAccess.Interfaces.Dtos;

public class LocationRouteDto
{
    public int FromPointId { get; set; }

    public int ToPointId { get; set; }

    public TimeSpan Duration { get; set; }

    public double Distance { get; set; }
}
