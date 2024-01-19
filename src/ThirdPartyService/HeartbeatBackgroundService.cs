using Apollo.Core;
using Apollo.Core.Messaging;
using Apollo.Core.Messaging.Events;

namespace ThirdPartyService;

public class HeartbeatBackgroundService :BackgroundService
{
    private readonly IRemotePublisher publisher;
    public HeartbeatBackgroundService(IRemotePublisherFactory remotePublisherFactory)
    {
        this.publisher = remotePublisherFactory.CreatePublisher("DashboardEndpoint");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hearbeat = new HeartbeatEvent { Id = "ThirdPartySystem", DisplayName = "Third Party System" };
        while (true)
        {
            hearbeat.UtcTimestamp = DateTime.UtcNow;
            Console.WriteLine($"Sending heartbeat for {hearbeat.DisplayName}...");
            await publisher.BroadcastAsync(hearbeat, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
public record HeartbeatEvent : IEvent
{
    public string Id { get; set; } // Unique identifier for the system
    public string DisplayName { get; set; } // Human-readable name of the system
    public DateTime UtcTimestamp { get; set; } // Timestamp of when the heartbeat was received
}