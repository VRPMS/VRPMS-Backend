namespace VRPMS.DataContracts.Responses;

public class GetCarsGridResponse
{
    public int Id { get; set; }

    public int CapacityOverloadPenalty { get; set; }

    public int MaxCapacityOverloadPenalty { get; set; }

    public BaseTimeWindowResponse CarWorkHours { get; set; } = null!;

    public int OverWorkPenalty { get; set; }

    public IEnumerable<int?> RouteTemplate { get; set; } = null!;

    public IEnumerable<BaseCarCapacityResponse> CarCapacities { get; set; } = null!;
}
