namespace VRPMS.DataAccess.Interfaces.Dtos;

public class LocationDto
{
    public int Id { get; set; }

    public int LocationTypeId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public TimeSpan ServiceTime { get; set; }

    public int LatePenalty { get; set; }

    public int WaitPenalty { get; set; }
}
