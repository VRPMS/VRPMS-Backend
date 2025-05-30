using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ILocationsRepository
{
    Task<IEnumerable<GetLocationsGridResponse>> Get(GetLocationsGridRequest request, CancellationToken cancellationToken = default);

    Task<IEnumerable<BaseRouteResponse>> GetRoutes(CancellationToken cancellationToken = default);

    Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default);

    Task<bool> LocationsBulkCopy(List<LocationDto> locations, CancellationToken cancellationToken = default);

    Task<bool> LocationDemandsBulkCopy(List<LocationDemandDto> locationDemands, CancellationToken cancellationToken = default);

    Task<bool> LocationTimeWindowsBulkCopy(List<LocationTimeWindowDto> locationTimeWindows, CancellationToken cancellationToken = default);

    Task<bool> LocationRoutesBulkCopy(List<LocationRouteDto> locationRoutes, CancellationToken cancellationToken = default);

    Task<bool> LocationSupplyChainsBulkCopy(List<LocationSupplyChainDto> locationSupplyChains, CancellationToken cancellationToken = default);
}