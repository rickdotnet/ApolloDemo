using Apollo.Core.Messaging.Events;

namespace Dashboard.Endpoints;

public record HeartbeatEvent : IEvent
{
    public string Id { get; set; }
    public string DisplayName { get; set; } 
    public DateTime UtcTimestamp { get; set; }
}