namespace VRPMS.DataAccess.Interfaces.Dtos;

public class CarCapacityDto
{
    public int CarId { get; set; }

    public int DemandId { get; set; }

    public double Capacity { get; set; }

    public double MaxCapacity { get; set; }
}
