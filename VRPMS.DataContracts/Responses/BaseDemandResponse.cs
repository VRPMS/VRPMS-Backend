namespace VRPMS.DataContracts.Responses;

public class BaseDemandResponse
{
    public int DemandId { get; set; }

    public string DemandName { get; set; } = null!;

    public double DemandValue { get; set; }
}
