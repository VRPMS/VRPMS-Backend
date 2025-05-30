namespace VRPMS.DataAccess.Interfaces.Dtos;

public class SolutionRouteVisitDto
{
    public int SolutionRouteId { get; set; }

    public int SequenceNumber { get; set; }

    public int LocationId { get; set; }

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan? DepartureTime { get; set; }
}
