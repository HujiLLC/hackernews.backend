namespace HackerNews.Backend.Configuration;

public class HackerNewsApiSettings
{
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0";
    public int CacheDurationMinutes { get; set; } = 5;
    public int MaxConcurrentRequests { get; set; } = 10;
}
