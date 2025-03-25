using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataContracts.Requests;
using VRPMS.DataContracts.Responses;

namespace VRPMS.DataAccess.Repositories;

public class TestRepository : ITestRepository
{
    public async Task<TestResponse> GetTestResponse(TestRequest request)
    {
        return await Task.FromResult(new TestResponse
        {
            Message = request.Message
        });
    }
}