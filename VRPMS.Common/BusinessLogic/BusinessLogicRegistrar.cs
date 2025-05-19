using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRPMS.DataAccess;

namespace VRPMS.Common.BusinessLogic;

public static class BusinessLogicRegistrar
{
    public static void RegisterDefaultDatabase(
        #nullable disable
        this IServiceCollection services, IConfiguration configuration)
    {
        LinqToDB.Common.Configuration.Linq.CompareNullsAsValues = false;

        services.AddLinqToDBContext<AppDataConnection>((provider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            return options.UsePostgreSQL(connectionString).UseDefaultLogging(provider);
        });
    }
}
