namespace VRPMS.DataContracts.Responses;

public class BaseCarCapacityResponse
{
    public int DemandId { get; set; }

    public string DemandName { get; set; } = null!;

    public double Capacity { get; set; }

    public double MaxCapacity { get; set; }
}
