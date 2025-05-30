namespace VRPMS.DataContracts.Responses;

public class GetSolutionRouteResponse
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public List<SolutionRouteVisitResponse> Visits { get; set; } = new();
}
