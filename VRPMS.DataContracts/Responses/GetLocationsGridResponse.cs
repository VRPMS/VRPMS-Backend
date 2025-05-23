namespace VRPMS.DataContracts.Responses;

public class GetLocationsGridResponse
{
    public int Id { get; set; }
    
    public BaseTypeResponse PointType { get; set; } = null!;
   
    public double Latitude { get; set; }

    public double Longitude { get; set; }
    
    public TimeSpan ServiceTime { get; set; }
    
    public int PenaltyLate { get; set; }
    
    public int PenaltyWait { get; set; }

    public IEnumerable<BaseDemandResponse> Demands { get; set; } = null!;

    public IEnumerable<BaseTimeWindowResponse>? TimeWindows { get; set; } = null!;
}