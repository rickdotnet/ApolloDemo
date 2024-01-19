using Apollo.Core.Messaging.Events;

namespace Dashboard.Endpoints;

public record AutomatedTestResultEvent : IEvent
{
    public string TestId { get; set; }
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public DateTime UtcTimestamp { get; set; }
}