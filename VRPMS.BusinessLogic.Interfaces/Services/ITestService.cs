using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Interfaces.Services;

public interface ITestService
{
    public Task<TestResponse> GetTestResponse(TestRequest request);
}