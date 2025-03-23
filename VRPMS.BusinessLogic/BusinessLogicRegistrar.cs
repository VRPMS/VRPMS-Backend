using Microsoft.Extensions.DependencyInjection;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.BusinessLogic.Services;
using VRPMS.Common.Services;

namespace VRPMS.BusinessLogic;

public class BusinessLogicRegistrar : IRegistrable
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<ITestService, TestService>();
    }
}