using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Interfaces.Services;

public interface ILocationsService
{
    Task<IEnumerable<GetLocationsGridResponse>> GetLocationsGrid(GetLocationsGridRequest request);
}