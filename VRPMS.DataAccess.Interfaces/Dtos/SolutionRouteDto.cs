namespace VRPMS.DataAccess.Interfaces.Dtos;

public class SolutionRouteDto
{
    public int SolutionId { get; set; }

    public int CarId { get; set; }

    public List<SolutionRouteVisitDto> SolutionRouteVisits { get; set; } = [];
}
