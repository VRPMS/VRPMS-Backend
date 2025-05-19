using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Services;

internal class LocationsService(
    ILocationsRepository locationsRepository)
    : ILocationsService
{
    public async Task<IEnumerable<GetLocationsGridResponse>> GetLocationsGrid(GetLocationsGridRequest request)
    {
        return await locationsRepository.Get(request);
    }
}