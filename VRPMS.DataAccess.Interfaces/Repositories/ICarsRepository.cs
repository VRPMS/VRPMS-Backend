using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ICarsRepository
{
    Task<IEnumerable<GetCarsGridResponse>> Get(GetCarsGridRequest request, CancellationToken cancellationToken = default);

    Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default);
}
