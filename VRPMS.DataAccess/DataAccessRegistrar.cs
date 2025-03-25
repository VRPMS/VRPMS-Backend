using Microsoft.Extensions.DependencyInjection;
using VRPMS.Common.Services;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataAccess.Repositories;

namespace VRPMS.DataAccess;

public class DataAccessRegistrar : IRegistrable
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<ITestRepository, TestRepository>();
    }
}