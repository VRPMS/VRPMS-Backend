using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Services;

public class TestService(
    ITestRepository testRepository)
    : ITestService
{
    public async Task<TestResponse> GetTestResponse(TestRequest request)
    {
        return await testRepository.GetTestResponse(request);
    }
}