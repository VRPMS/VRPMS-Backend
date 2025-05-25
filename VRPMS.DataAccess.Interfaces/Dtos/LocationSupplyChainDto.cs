namespace VRPMS.DataAccess.Interfaces.Dtos;

public class LocationSupplyChainDto
{
    public int ClientId { get; set; }

    public int? WarehouseId { get; set; }

    public int? CrossDockId { get; set; }
}
