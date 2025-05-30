using Microsoft.Extensions.DependencyInjection;
using VRPMS.Common.Services;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.BusinessLogic.Services;
using VRPMS.BusinessLogic.Validators.BusinessValidators;
using VRPMS.BusinessLogic.Helpers;
using System.Reflection;

namespace VRPMS.BusinessLogic;

public class BusinessLogicRegistrar : IRegistrable
{
    public void Register(IServiceCollection services)
    {
        services.AddTransient<ILocationsService, LocationsService>();
        services.AddTransient<ICarsService, CarsService>();
        services.AddTransient<ILovsService, LovsService>();
        services.AddTransient<IDataService, DataService>();
        services.AddTransient<ISolutionsService, SolutionsService>();

        services.AddTransient<ExcelValidator>();

        services.AddTransient<ExcelParser>();
    }
}