using HackerNews.Backend.Models;

namespace HackerNews.Backend.Services;

public interface IHackerNewsService
{
    Task<List<int>> GetNewStoryIdsAsync();
    Task<Story?> GetStoryAsync(int id);
    Task<List<Story>> GetStoriesAsync(IEnumerable<int> ids);
}
