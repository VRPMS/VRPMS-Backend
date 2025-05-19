using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ILocationsRepository
{
    Task<IEnumerable<GetLocationsGridResponse>> Get(GetLocationsGridRequest request, CancellationToken cancellationToken = default);

    Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default);
}