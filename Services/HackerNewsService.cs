using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using HackerNews.Backend.Configuration;
using HackerNews.Backend.Models;

namespace HackerNews.Backend.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly HackerNewsApiSettings _settings;
    private readonly ILogger<HackerNewsService> _logger;
    private readonly SemaphoreSlim _semaphore;

    public HackerNewsService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<HackerNewsApiSettings> settings,
        ILogger<HackerNewsService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
        _semaphore = new SemaphoreSlim(_settings.MaxConcurrentRequests, _settings.MaxConcurrentRequests);
    }

    public async Task<List<int>> GetNewStoryIdsAsync()
    {
        const string cacheKey = "newest_story_ids";
        
        if (_cache.TryGetValue(cacheKey, out List<int>? cachedIds) && cachedIds != null)
        {
            _logger.LogDebug("Returning cached newest story IDs");
            return cachedIds;
        }

        try
        {
            _logger.LogInformation("Fetching newest story IDs from Hacker News API");
            
            var response = await _httpClient.GetStringAsync($"{_settings.BaseUrl}/newstories.json");
            var storyIds = JsonSerializer.Deserialize<List<int>>(response) ?? new List<int>();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CacheDurationMinutes),
                Priority = CacheItemPriority.High
            };

            _cache.Set(cacheKey, storyIds, cacheOptions);
            
            _logger.LogInformation("Successfully fetched {Count} newest story IDs", storyIds.Count);
            return storyIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching newest story IDs");
            return new List<int>();
        }
    }

    public async Task<Story?> GetStoryAsync(int id)
    {
        var cacheKey = $"story_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Story? cachedStory) && cachedStory != null)
        {
            return cachedStory;
        }

        await _semaphore.WaitAsync();

        try
        {
            var response = await _httpClient.GetStringAsync($"{_settings.BaseUrl}/item/{id}.json");
            var story = JsonSerializer.Deserialize<Story>(response);

            if (story != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CacheDurationMinutes * 2), // Cache stories longer
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, story, cacheOptions);
            }

            return story;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching story {StoryId}", id);
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<Story>> GetStoriesAsync(IEnumerable<int> ids)
    {
        var tasks = ids.Select(GetStoryAsync);
        var stories = await Task.WhenAll(tasks);
        
        return stories
            .Where(story => story != null && !story.Deleted.GetValueOrDefault() && !story.Dead.GetValueOrDefault())
            .Cast<Story>()
            .ToList();
    }
}
