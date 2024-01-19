# ApolloDemo
A Dashboard Demo using [Apollo](https://github.com/rickdotnet/Apollo)

## How-To
Start a [NATS server](https://github.com/nats-io/nats-server) locally (or find/replace `localhost:4222`). Run the Dashboard. Run the "ThirdPartyService". Optionally, throw some bogus data around by running ZampleData.

## The relevant bits:

**Startup**
```cs
var config = new ApolloConfig("nats://localhost:4222");

builder.Services.AddScoped<HeartbeatService>();
builder.Services.AddScoped<AutomatedTestService>();
builder.Services.AddSingleton<IStateObserver, StateObserver>();
builder.Services
    .AddApollo(config)
    .WithEndpoints(x=>x.AddEndpoint<DashboardEndpoint>());
```

**Endpoint**
```cs
public record HeartbeatEvent(string Id, string DisplayName, DateTime UtcTimestamp) : IEvent;
public async ValueTask HandleEventAsync(HeartbeatEvent message, CancellationToken cancellationToken = default)
{
    logger.LogInformation("Received heartbeat from {DisplayName}", message.DisplayName);
    await heartbeatService.UpdateHeartbeatAsync(message);
}

public async ValueTask HandleEventAsync(AutomatedTestResultEvent message, CancellationToken cancellationToken = default)
{
    logger.LogInformation("Received test result from {DisplayName}", message.DisplayName);
    await automatedTestService.UpdateTestResultAsync(message);
    }
}
```

**Component**
```cs
protected override async Task OnInitializedAsync()
{
    heartbeats = await HeartbeatService.GetAllHeartbeatsAsync();
    heartbeatSubscription = StateObserver.Register<HeartbeatEvent>(OnHeartbeatUpdated);
}

private async Task OnHeartbeatUpdated(HeartbeatEvent heartbeat)
{
    await InvokeAsync(() =>
    {
        Console.WriteLine("InvokeAsync");
        heartbeats[heartbeat.Id] = heartbeat;
        StateHasChanged();
    });
}
```
