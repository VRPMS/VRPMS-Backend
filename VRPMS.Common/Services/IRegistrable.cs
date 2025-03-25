using Microsoft.Extensions.DependencyInjection;

namespace VRPMS.Common.Services;

public interface IRegistrable
{
    void Register(IServiceCollection services);
}