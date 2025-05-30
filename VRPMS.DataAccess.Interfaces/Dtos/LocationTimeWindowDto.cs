namespace VRPMS.DataAccess.Interfaces.Dtos;

public class LocationTimeWindowDto
{
    public int LocationId { get; set; }

    public TimeSpan? WindowStart { get; set; }

    public TimeSpan? WindowEnd { get; set; }
}
