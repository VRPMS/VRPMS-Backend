using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Services;

internal class CarsService(
    ICarsRepository carsRepository)
    : ICarsService
{
    public async Task<IEnumerable<GetCarsGridResponse>> GetCarsGrid(GetCarsGridRequest request)
    {
        return await carsRepository.Get(request);
    }
}
