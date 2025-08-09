using HackerNews.Backend.Models;

namespace HackerNews.Backend.Services;

public class StoryService : IStoryService
{
    private readonly IHackerNewsService _hackerNewsService;
    private readonly ILogger<StoryService> _logger;

    public StoryService(IHackerNewsService hackerNewsService, ILogger<StoryService> logger)
    {
        _hackerNewsService = hackerNewsService;
        _logger = logger;
    }

    public async Task<PaginatedStories> GetNewestStoriesAsync(SearchParameters parameters)
    {
        try
        {
            _logger.LogInformation("Getting newest stories - Page: {Page}, PageSize: {PageSize}, Search: {Search}", 
                parameters.Page, parameters.PageSize, parameters.Search);

            var storyIds = await _hackerNewsService.GetNewStoryIdsAsync();
            
            if (!storyIds.Any())
            {
                _logger.LogWarning("No story IDs returned from Hacker News API");
                return new PaginatedStories();
            }

            // Get all stories first for accurate filtering
            var allStories = await _hackerNewsService.GetStoriesAsync(storyIds.Take(500)); // Limit to first 500 for performance
            
            // Filter by search term if provided
            var filteredStories = allStories;
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLowerInvariant();
                filteredStories = allStories
                    .Where(story => story.Title.ToLowerInvariant().Contains(searchTerm) ||
                                   (story.Text?.ToLowerInvariant().Contains(searchTerm) ?? false))
                    .ToList();
            }

            // Calculate pagination
            var totalCount = filteredStories.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
            var skip = (parameters.Page - 1) * parameters.PageSize;
            
            var paginatedStories = filteredStories
                .Skip(skip)
                .Take(parameters.PageSize)
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} stories out of {Total} total", 
                paginatedStories.Count, totalCount);

            return new PaginatedStories
            {
                Stories = paginatedStories,
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting newest stories");
            return new PaginatedStories();
        }
    }

    public async Task<PaginatedStories> SearchStoriesAsync(SearchParameters parameters)
    {
        try
        {
            _logger.LogInformation("Searching stories - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
                parameters.Query, parameters.Page, parameters.PageSize);

            if (string.IsNullOrWhiteSpace(parameters.Query))
            {
                // If no query provided, return newest stories
                return await GetNewestStoriesAsync(new SearchParameters 
                { 
                    Page = parameters.Page, 
                    PageSize = parameters.PageSize 
                });
            }

            var storyIds = await _hackerNewsService.GetNewStoryIdsAsync();
            var allStories = await _hackerNewsService.GetStoriesAsync(storyIds.Take(500));
            
            var searchTerm = parameters.Query.ToLowerInvariant();
            var filteredStories = allStories
                .Where(story => story.Title.ToLowerInvariant().Contains(searchTerm) ||
                               (story.Text?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                               story.By.ToLowerInvariant().Contains(searchTerm))
                .ToList();

            var totalCount = filteredStories.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
            var skip = (parameters.Page - 1) * parameters.PageSize;
            
            var paginatedStories = filteredStories
                .Skip(skip)
                .Take(parameters.PageSize)
                .ToList();

            _logger.LogInformation("Search returned {Count} stories out of {Total} total", 
                paginatedStories.Count, totalCount);

            return new PaginatedStories
            {
                Stories = paginatedStories,
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stories");
            return new PaginatedStories();
        }
    }
}
