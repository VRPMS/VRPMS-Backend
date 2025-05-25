using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Services;

internal class LovsService(
    ILocationsRepository locationsRepository,
    IDemandsRepository demandsRepository)
    : ILovsService
{
    public async Task<IEnumerable<BaseTypeResponse>> GetDemandTypesLov()
    {
        return await demandsRepository.GetTypesLov();
    }

    public async Task<IEnumerable<BaseTypeResponse>> GetLocationTypesLov()
    {
        return await locationsRepository.GetTypesLov();
    }
}
