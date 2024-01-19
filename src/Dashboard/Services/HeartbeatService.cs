using Apollo.Core.Hosting;
using ZiggyCreatures.Caching.Fusion;
using Dashboard.Endpoints;


namespace Dashboard.Services;

public class HeartbeatService
{
    private readonly IFusionCache cache;
    private readonly ILogger<HeartbeatService> logger;
    private readonly IStateObserver stateObserver;
    private const string HeartbeatCacheKey = "Heartbeats";
    
    public HeartbeatService(IFusionCache cache, ILogger<HeartbeatService> logger, IStateObserver stateObserver)
    {
        this.cache = cache;
        this.logger = logger;
        this.stateObserver = stateObserver;
    }

    public async Task UpdateHeartbeatAsync(HeartbeatEvent heartbeat)
    {
        var heartbeats = await cache.GetOrSetAsync<Dictionary<string, HeartbeatEvent>>(HeartbeatCacheKey, 
            _ => Task.FromResult(new Dictionary<string, HeartbeatEvent>()));

        heartbeats![heartbeat.Id] = heartbeat;
        
        
        // Update the cache with the new dictionary of heartbeats
        await cache.SetAsync(HeartbeatCacheKey, heartbeats);

        logger.LogInformation("Updated heartbeat for {DisplayName}", heartbeat.DisplayName);
        await stateObserver.NotifyAsync(heartbeat);
    }

    public async Task<Dictionary<string, HeartbeatEvent>> GetAllHeartbeatsAsync()
    {
        // Retrieve the dictionary of all heartbeats from the cache
        var results =  await cache.GetOrDefaultAsync(HeartbeatCacheKey, new Dictionary<string, HeartbeatEvent>());
        return results;
    }

    private static string GetCacheKey(string id) => $"{typeof(HeartbeatEvent).Name}";
}