using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ICarsRepository
{
    Task<IEnumerable<GetCarsGridResponse>> Get(CancellationToken cancellationToken = default);
}
