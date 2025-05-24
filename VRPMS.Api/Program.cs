using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using VRPMS.Api.Filters;
using VRPMS.Api.Settings;
using VRPMS.Composition.BusinessLogic;
using VRPMS.Composition.Helpers;
using VRPMS.DataContracts.Constants.ControllerConstants;

namespace VRPMS.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:8080");

        builder.Services.AddOpenApi();
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<BusinessExceptionFilter>();
        });
        builder.Services.AddHealthChecks();

        var scalarConfig = builder.Configuration
            .GetSection(SectionConstants.ScalarApi)
            .Get<ScalarApiSettings>() ?? new ScalarApiSettings();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
            );
        });

        builder.Services.Configure<ForwardedHeadersOptions>(opts =>
        {
            opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            opts.KnownNetworks.Clear();
            opts.KnownProxies.Clear();
        });

        builder.Services.RegisterDefaultDatabase(builder.Configuration);
        builder.Services.RegisterAssemblies();

        var app = builder.Build();

        app.UseForwardedHeaders();
        app.UseCors();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        app.MapOpenApi();
        app.MapScalarApiReference(endpointPrefix: "scalar", options =>
        {
            options.WithTitle(scalarConfig.Title);
            options.WithTheme(Enum.Parse<ScalarTheme>(scalarConfig.Theme));
            options.WithSidebar(scalarConfig.Sidebar);
        });

        app.MapHealthChecks(HealthControllerConstants.Prefix);

        app.MapControllers();

        app.Run();
    }
}