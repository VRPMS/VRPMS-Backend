namespace VRPMS.DataAccess.Interfaces.Dtos;

public class SolutionDto
{
    public decimal TotalScore { get; set; }

    public List<SolutionRouteDto> SolutionRoutes = [];
}
