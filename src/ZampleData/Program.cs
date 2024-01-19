using Apollo.Core;
using Apollo.Core.Configuration;
using Apollo.Core.Hosting;
using Apollo.Core.Messaging;
using Apollo.Core.Messaging.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var config = new ApolloConfig("nats://localhost:4222");

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApollo(config)
    .WithRemotePublishing();

var host = builder.Build();
var publisherFactory = host.Services.GetRequiredService<IRemotePublisherFactory>();

var remoteDispatcher = publisherFactory.CreatePublisher("DashboardEndpoint");

var systemTasks = new List<Task>
{
    SimulateHeartbeatAsync(remoteDispatcher, new HeartbeatEvent { Id = "System1", DisplayName = "Main System" }, 5),
    SimulateHeartbeatAsync(remoteDispatcher, new HeartbeatEvent { Id = "System2", DisplayName = "Backup System" }, 5),
    SimulateHeartbeatAsync(remoteDispatcher, new HeartbeatEvent { Id = "System3", DisplayName = "Analytics System" }, 15)
};

var testTasks = new List<Task>
{
    SimulateAutomatedTestAsync(remoteDispatcher, new AutomatedTestResultEvent { TestId = "TestSet1", DisplayName = "Integration Test" }, 10),
    SimulateAutomatedTestAsync(remoteDispatcher, new AutomatedTestResultEvent { TestId = "TestSet2", DisplayName = "Unit Test" } ,20),
    SimulateAutomatedTestAsync(remoteDispatcher, new AutomatedTestResultEvent { TestId = "TestSet3", DisplayName = "End-to-End Test" },20),
};

await Task.WhenAll(systemTasks.Concat(testTasks));

Console.WriteLine("Closing");

static async Task SimulateHeartbeatAsync(IRemotePublisher remoteDispatcher, HeartbeatEvent system, int delayInSeconds)
{
    while (true)
    {
        system.UtcTimestamp = DateTime.UtcNow;
        Console.WriteLine($"Sending heartbeat for {system.DisplayName}...");
        await remoteDispatcher.BroadcastAsync(system, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
    }
}

static async Task SimulateAutomatedTestAsync(IRemotePublisher remoteDispatcher, AutomatedTestResultEvent test, int delayInSeconds)
{
    while (true)
    {
        test.Id = Guid.NewGuid().ToString();
        test.Description = $"This is a test description ({test.Id})";
        test.Status = GetRandomStatus();
        test.UtcTimestamp = DateTime.UtcNow;
        
        Console.WriteLine($"Sending test result for {test.DisplayName}...");
        await remoteDispatcher.BroadcastAsync(test, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
    }
}

static string GetRandomStatus()
{
    var statuses = new[] { "Pass", "Fail", "Running", "Skipped" };
    var random = new Random();
    return statuses[random.Next(statuses.Length)];
}