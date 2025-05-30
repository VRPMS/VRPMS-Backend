namespace VRPMS.DataContracts.Responses;

public class BaseRouteResponse
{
    public int FromLocationId { get; set; }

    public int ToLocationId { get; set; }

    public TimeSpan Duration { get; set; }

    public double Distance { get; set; }
}
