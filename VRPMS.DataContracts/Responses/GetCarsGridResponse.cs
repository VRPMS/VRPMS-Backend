namespace VRPMS.DataContracts.Responses;

public class GetCarsGridResponse
{
    public int Id { get; set; }

    public BaseTypeResponse CarType { get; set; } = null!;

    public int CapacityOverloadPenalty { get; set; }

    public int MaxCapacityOverloadPenalty { get; set; }

    public int OverWorkPenalty { get; set; }

    public IEnumerable<int?> RouteTemplate { get; set; } = null!;

    public IEnumerable<BaseCarCapacityResponse> CarCapacities { get; set; } = null!;

    public IEnumerable<BaseTimeWindowResponse>? CarWorkHours { get; set; } = null!;
}
