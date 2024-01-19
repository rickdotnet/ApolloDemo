using Apollo.Core.Hosting;
using Dashboard.Endpoints;
using ZiggyCreatures.Caching.Fusion;

namespace Dashboard.Services;

public class AutomatedTestService
{
    private readonly IFusionCache cache;
    private readonly ILogger<AutomatedTestService> logger;
    private readonly AutomatedTestApi testApi;
    private readonly IStateObserver stateObserver;
    private const string AutomatedTestCacheKey = "AutomatedTests";

    public AutomatedTestService(IFusionCache cache, ILogger<AutomatedTestService> logger, AutomatedTestApi testApi,
        IStateObserver stateObserver)
    {
        this.cache = cache;
        this.logger = logger;
        this.testApi = testApi;
        this.stateObserver = stateObserver;
    }

    public async Task UpdateTestResultAsync(AutomatedTestResultEvent testResult)
    {
        // Retrieve the dictionary of lists of test results, or initialize a new one if it doesn't exist
        var automatedTests =
            await cache.GetOrDefaultAsync<Dictionary<string, List<AutomatedTestResultEvent>>>(AutomatedTestCacheKey);

        // If the list for the given Id doesn't exist, initialize it
        if (!automatedTests!.TryGetValue(testResult.TestId, out var testResults))
        {
            testResults = new List<AutomatedTestResultEvent>();
        }

        if (!testResults.Any(x => x.Id == testResult.Id))
            testResults.Add(testResult);
        
        automatedTests[testResult.TestId] = testResults.OrderByDescending(x=>x.UtcTimestamp).Take(3).ToList();
        // // Add the new test result to the list for the given Id
        // testResults.Add(testResult);

        // Update the cache with the new dictionary of lists of test results
        await cache.SetAsync(AutomatedTestCacheKey, automatedTests);

        logger.LogInformation("Updated test result for {DisplayName}", testResult.DisplayName);
        await stateObserver.NotifyAsync(testResult);
    }

    public async Task<Dictionary<string, List<AutomatedTestResultEvent>>> GetAllTestResultsAsync()
    {
        // Retrieve the dictionary of lists of all test results from the cache
        var result = await cache.GetOrSetAsync(AutomatedTestCacheKey,
            async _ =>
            {
                var results = await testApi.GetAllTestResults();

                return results
                    .GroupBy(testResult => testResult.TestId)
                    .ToDictionary(group => group.Key, group => group.ToList());
            });
        return result!;
    }
}