using System.Collections.Concurrent;
using Apollo.Core;
using Apollo.Core.Messaging;
using Apollo.Core.Messaging.Events;

namespace ThirdPartyService;

public class TestResultService
{
    private readonly IRemotePublisher publisher;

    public TestResultService(IRemotePublisherFactory remotePublisherFactory)
    {
        publisher = remotePublisherFactory.CreatePublisher("DashboardEndpoint");
    }

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AutomatedTestResultEvent>>
        testResultsByTestId = new();

    public IEnumerable<AutomatedTestResultEvent> GetAllTestResults()
    {
        return testResultsByTestId.Values
            .SelectMany(testRuns => testRuns.Values)
            .OrderBy(testRun => testRun.UtcTimestamp)
            .ToList();
    }

    public IEnumerable<AutomatedTestResultEvent>? GetTestResultsByTestId(string testId)
    {
        if (testResultsByTestId.TryGetValue(testId, out var testRuns))
        {
            return testRuns.Values;
        }

        return null;
    }

    public AutomatedTestResultEvent? AddTestResult(AutomatedTestResultEvent newTestResult, bool publish = true)
    {
        if (newTestResult.Id is null || newTestResult.TestId is null)
        {
            return null;
        }

        var testRuns =
            testResultsByTestId.GetOrAdd(newTestResult.TestId,
                _ => new ConcurrentDictionary<string, AutomatedTestResultEvent>()
            );
        
        if (testRuns.TryAdd(newTestResult.Id, newTestResult))
        {
            // we can get away with this in a singleton
            if (publish)
                publisher.BroadcastAsync(newTestResult, new CancellationToken());

            return newTestResult;
        }

        return null;
    }
}

public record AutomatedTestResultEvent(
    string TestId,
    string Id,
    string DisplayName,
    string Status,
    string Description,
    DateTime UtcTimestamp) : IEvent;