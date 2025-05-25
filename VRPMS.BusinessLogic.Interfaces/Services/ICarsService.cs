using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Interfaces.Services;

public interface ICarsService
{
    Task<IEnumerable<GetCarsGridResponse>> GetCarsGrid();
}
