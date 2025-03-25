using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Interfaces.Repositories;

public interface ITestRepository
{
    public Task<TestResponse> GetTestResponse(TestRequest request);
}