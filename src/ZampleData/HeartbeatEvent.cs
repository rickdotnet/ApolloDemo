using Apollo.Core.Messaging.Events;

public record HeartbeatEvent : IEvent
{
    public string Id { get; set; } // Unique identifier for the system
    public string DisplayName { get; set; } // Human-readable name of the system
    public DateTime UtcTimestamp { get; set; } // Timestamp of when the heartbeat was received
}