using Microsoft.Extensions.DependencyInjection;
using VRPMS.Common.Services;
using VRPMS.DataAccess.Functions;
using VRPMS.DataAccess.Interfaces.Functions;
using VRPMS.DataAccess.Interfaces.Repositories;
using VRPMS.DataAccess.Repositories;

namespace VRPMS.DataAccess;

public class DataAccessRegistrar : IRegistrable
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<AppDataConnection>();

        services.AddTransient<ILocationsRepository, LocationsRepository>();
        services.AddTransient<ICarsRepository, CarsRepository>();
        services.AddTransient<IDemandsRepository, DemandsRepository>();

        services.AddTransient<IVrpmsProcedures, VrpmsProcedures>();
        services.AddTransient<IVrpmsFunctions, VrpmsFunctions>();
    }
}