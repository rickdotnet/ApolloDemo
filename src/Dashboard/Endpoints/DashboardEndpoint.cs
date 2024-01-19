using Apollo.Core.Messaging.Events;
using Dashboard.Services;

namespace Dashboard.Endpoints;

public class DashboardEndpoint : IListenFor<HeartbeatEvent>, IListenFor<AutomatedTestResultEvent>
{
    private readonly ILogger<DashboardEndpoint> logger;
    private readonly HeartbeatService heartbeatService;
    private readonly AutomatedTestService automatedTestService;

    public DashboardEndpoint(
        ILogger<DashboardEndpoint> logger,
        HeartbeatService heartbeatService,
        AutomatedTestService automatedTestService)
    {
        this.logger = logger;
        this.heartbeatService = heartbeatService;
        this.automatedTestService = automatedTestService;
    }

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