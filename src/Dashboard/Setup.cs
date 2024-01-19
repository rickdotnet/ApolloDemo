using Apollo.Core.Configuration;
using Apollo.Core.Hosting;
using Dashboard.Components;
using Dashboard.Endpoints;
using Dashboard.Services;
using FusionLite;
using Refit;
using Serilog;
using Serilog.Events;
using ZiggyCreatures.Caching.Fusion;

namespace Dashboard;

internal static class Setup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // pre-app-startup logger
        Log.Logger = new LoggerConfiguration()
            .ConfigureLogger()
            .CreateBootstrapLogger();

        // post-app-startup logger
        builder.Host.UseSerilog(
            (_, services, configuration) => configuration
                .ReadFrom.Services(services)
                .ConfigureLogger()
        );

        var config = new ApolloConfig("nats://localhost:4222");
        //var durableConfig = new DurableConfig { IsDurableConsumer = true };

        builder.Services.AddScoped<HeartbeatService>();
        builder.Services.AddScoped<AutomatedTestService>();
        builder.Services.AddSingleton<IStateObserver, StateObserver>();
        builder.Services
            .AddApollo(config)
            .WithEndpoints(x => x.AddEndpoint<DashboardEndpoint>());
            //.WithEndpoints(x => x.AddEndpoint<DashboardEndpoint>(cfg => cfg.DurableConfig = durableConfig ));

        var apiUrl = "https://localhost:7161";

        builder.Services.AddRefitClient<AutomatedTestApi>()
            .ConfigureHttpClient(c => { c.BaseAddress = new Uri(apiUrl); });

        var fusion = builder.Services
            .AddFusionCache()
            .WithFusionLite(builder.Services)
            .WithDefaultEntryOptions(new FusionCacheEntryOptions(TimeSpan.FromDays(30)))
            .WithSystemTextJsonSerializer();

        // standard services
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    private static LoggerConfiguration ConfigureLogger(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                LogEventLevel.Debug,
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
    }
}