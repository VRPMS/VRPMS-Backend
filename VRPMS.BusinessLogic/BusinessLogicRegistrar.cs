using Microsoft.Extensions.DependencyInjection;
using VRPMS.Common.Services;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.BusinessLogic.Services;
using VRPMS.BusinessLogic.Validators.BusinessValidators;
using VRPMS.BusinessLogic.Helpers;

namespace VRPMS.BusinessLogic;

public class BusinessLogicRegistrar : IRegistrable
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<ILocationsService, LocationsService>();
        services.AddScoped<ICarsService, CarsService>();
        services.AddScoped<ILovsService, LovsService>();
        services.AddScoped<IDataService, DataService>();

        services.AddScoped<ExcelValidator>();

        services.AddScoped<ExcelParser>();
    }
}