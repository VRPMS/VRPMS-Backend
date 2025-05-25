namespace VRPMS.DataAccess.Interfaces.Dtos;

public class DataDto
{
    public List<DemandTypeDto> DemandTypes { get; set; } = [];

    public List<LocationDto> Locations { get; set; } = [];

    public List<LocationDemandDto> LocationDemands { get; set; } = [];

    public List<LocationTimeWindowDto> LocationsTimeWindows { get; set; } = [];

    public List<LocationRouteDto> LocationRoutes { get; set; } = [];
   
    public List<LocationSupplyChainDto> LocationSupplyChains { get; set; } = [];

    public List<CarDto> Cars { get; set; } = [];

    public List<CarCapacityDto> CarCapacities { get; set; } = [];
}
