using Apollo.Core.Messaging.Events;

public record AutomatedTestResultEvent : IEvent
{
    public string TestId { get; set; } // Unique identifier for the test run
    public string Id { get; set; } // Unique identifier for the test
    public string DisplayName { get; set; } // Human-readable name of the test
    public string Status { get; set; } // Status of the test (e.g., "Pass", "Fail")
    public string Description { get; set; } // Arbitrary description of the test run
    public DateTime UtcTimestamp { get; set; } // Timestamp of when the test result was received
}