using VRPMS.DataAccess.Interfaces.Dtos;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ICarsRepository
{
    Task<IEnumerable<GetCarsGridResponse>> Get(CancellationToken cancellationToken = default);

    Task<bool> CarsBulkCopy(List<CarDto> cars, CancellationToken cancellationToken = default);

    Task<bool> CarCapacitiesBulkCopy(List<CarCapacityDto> carCapacities, CancellationToken cancellationToken = default);
}
