using Scalar.AspNetCore;
using VRPMS.Common.Helpers;

namespace VRPMS.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();
        builder.Services.RegisterAssemblies();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(endpointPrefix: "api", options =>
            {
                options.WithTitle("VRPMS API");
                options.WithTheme(ScalarTheme.BluePlanet);
                options.WithSidebar(true);
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.MapControllers();

        app.Run();
    }
}