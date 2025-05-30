namespace VRPMS.DataContracts.Responses;

public class SolutionRouteVisitResponse
{
    public int LocationId { get; set; }

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan? DepartureTime { get; set; }
}
