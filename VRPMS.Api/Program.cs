using Microsoft.AspNetCore.HttpOverrides;
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

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.MapOpenApi();

        app.MapScalarApiReference(endpointPrefix: "scalar", options =>
        {
            options.WithTitle("VRPMS API");
            options.WithTheme(ScalarTheme.BluePlanet);
            options.WithSidebar(true);
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.MapGet("/", () => Results.Ok("VRPMS API is running"));

        app.MapControllers();

        app.Run();
    }
}