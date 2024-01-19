using Apollo.Core.Configuration;
using Apollo.Core.Hosting;
using Serilog;
using Serilog.Events;

namespace ThirdPartyService;

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

        builder.Services
            .AddApollo(config)
            .WithRemotePublishing();
        //.WithEndpoints(x => x.AddEndpoint<DashboardEndpoint>());

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<TestResultService>(); // Register the new service
        builder.Services.AddHostedService<HeartbeatBackgroundService>();


        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        var testResultService = app.Services.GetRequiredService<TestResultService>(); // Resolve the service

        app.MapGet("/automatedtestresults", () =>
            {
                var allTestRuns = testResultService.GetAllTestResults();
                return Results.Ok(allTestRuns);
            })
            .WithName("GetAllAutomatedTestResults")
            .WithOpenApi();

        app.MapGet("/automatedtestresults/{testId}", (string testId) =>
            {
                var testRuns = testResultService.GetTestResultsByTestId(testId);
                if (testRuns is null)
                {
                    return Results.NotFound("Test ID not found.");
                }

                return Results.Ok(testRuns);
            })
            .WithName("GetAutomatedTestResultsByTestId")
            .WithOpenApi();

        app.MapPost("/automatedtestresults", (AutomatedTestResultEvent newTestResult) =>
            {
                var result = testResultService.AddTestResult(newTestResult);
                if (result == null)
                {
                    return Results.BadRequest(
                        "The test result must have both a unique ID and a TestId, or a test result with the same ID already exists for this TestId.");
                }

                return Results.Created($"/automatedtestresults/{newTestResult.TestId}/{newTestResult.Id}",
                    newTestResult);
            })
            .WithName("PostAutomatedTestResult")
            .WithOpenApi();
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