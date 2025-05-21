using Scalar.AspNetCore;
using VRPMS.Common.Helpers;
using VRPMS.Composition.BusinessLogic;

namespace VRPMS.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:8080");

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();
        builder.Services.AddCors();
        builder.Services.RegisterDefaultDatabase(builder.Configuration);
        builder.Services.RegisterAssemblies();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(endpointPrefix: "scalar", options =>
            { 
                options.WithTitle("VRPMS API");
                options.WithTheme(ScalarTheme.BluePlanet);
                options.WithSidebar(true);
            });
        }

        app.UseHttpsRedirection();
        
        app.UseCors();

        app.UseRouting();

        app.MapControllers();

        app.Run();
    }
}