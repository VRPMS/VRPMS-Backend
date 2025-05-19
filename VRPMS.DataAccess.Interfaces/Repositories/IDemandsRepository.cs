using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface IDemandsRepository
{
    Task<IEnumerable<BaseTypeResponse>> GetTypesLov(CancellationToken cancellationToken = default);
}
