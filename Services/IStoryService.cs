using HackerNews.Backend.Models;

namespace HackerNews.Backend.Services;

public interface IStoryService
{
    Task<PaginatedStories> GetNewestStoriesAsync(SearchParameters parameters);
    Task<PaginatedStories> SearchStoriesAsync(SearchParameters parameters);
}
