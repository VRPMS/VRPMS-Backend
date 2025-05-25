namespace VRPMS.DataAccess.Interfaces.Dtos;

public class CarDto
{
    public int Id { get; set; }

    public int CapacityOverloadPenalty { get; set; }

    public int MaxCapacityOverloadPenalty { get; set; }

    public TimeSpan? WorkStart { get; set; }

    public TimeSpan? WorkEnd { get; set; }

    public int OverWorkPenalty { get; set; }

    public int?[] RouteTemplate { get; set; } = [];
}
