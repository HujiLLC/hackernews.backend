using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using HackerNews.Backend.Services;
using HackerNews.Backend.Models;

namespace HackerNews.Backend.Tests.Services;

public class StoryServiceTests
{
    private readonly Mock<IHackerNewsService> _mockHackerNewsService;
    private readonly Mock<ILogger<StoryService>> _mockLogger;
    private readonly StoryService _storyService;

    public StoryServiceTests()
    {
        _mockHackerNewsService = new Mock<IHackerNewsService>();
        _mockLogger = new Mock<ILogger<StoryService>>();
        _storyService = new StoryService(_mockHackerNewsService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetNewestStoriesAsync_ShouldReturnPaginatedStories_WhenValidParameters()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3, 4, 5 };
        var stories = new List<Story>
        {
            new() { Id = 1, Title = "Story 1", Score = 100, By = "user1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 2, Title = "Story 2", Score = 200, By = "user2", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 3, Title = "Story 3", Score = 300, By = "user3", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var parameters = new SearchParameters { Page = 1, PageSize = 2 };

        _mockHackerNewsService.Setup(x => x.GetNewStoryIdsAsync())
            .ReturnsAsync(storyIds);
        _mockHackerNewsService.Setup(x => x.GetStoriesAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(stories);

        // Act
        var result = await _storyService.GetNewestStoriesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Stories.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetNewestStoriesAsync_ShouldFilterBySearch_WhenSearchTermProvided()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3 };
        var stories = new List<Story>
        {
            new() { Id = 1, Title = "Angular Tutorial", Score = 100, By = "user1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 2, Title = "React Guide", Score = 200, By = "user2", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 3, Title = "Vue.js Tips", Score = 300, By = "user3", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var parameters = new SearchParameters { Page = 1, PageSize = 10, Search = "angular" };

        _mockHackerNewsService.Setup(x => x.GetNewStoryIdsAsync())
            .ReturnsAsync(storyIds);
        _mockHackerNewsService.Setup(x => x.GetStoriesAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(stories);

        // Act
        var result = await _storyService.GetNewestStoriesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Stories.Should().HaveCount(1);
        result.Stories.First().Title.Should().Be("Angular Tutorial");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchStoriesAsync_ShouldReturnMatchingStories_WhenQueryProvided()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3 };
        var stories = new List<Story>
        {
            new() { Id = 1, Title = "JavaScript News", Score = 100, By = "jsdev", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 2, Title = "Python Tutorial", Score = 200, By = "pythoneer", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 3, Title = "C# Guide", Score = 300, By = "csharper", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var parameters = new SearchParameters { Page = 1, PageSize = 10, Query = "python" };

        _mockHackerNewsService.Setup(x => x.GetNewStoryIdsAsync())
            .ReturnsAsync(storyIds);
        _mockHackerNewsService.Setup(x => x.GetStoriesAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(stories);

        // Act
        var result = await _storyService.SearchStoriesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Stories.Should().HaveCount(1);
        result.Stories.First().Title.Should().Be("Python Tutorial");
    }

    [Fact]
    public async Task GetNewestStoriesAsync_ShouldReturnEmptyResult_WhenNoStoriesAvailable()
    {
        // Arrange
        var parameters = new SearchParameters { Page = 1, PageSize = 10 };

        _mockHackerNewsService.Setup(x => x.GetNewStoryIdsAsync())
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _storyService.GetNewestStoriesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Stories.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task SearchStoriesAsync_ShouldReturnNewestStories_WhenQueryIsEmpty()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2 };
        var stories = new List<Story>
        {
            new() { Id = 1, Title = "Story 1", Score = 100, By = "user1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new() { Id = 2, Title = "Story 2", Score = 200, By = "user2", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var parameters = new SearchParameters { Page = 1, PageSize = 10, Query = "" };

        _mockHackerNewsService.Setup(x => x.GetNewStoryIdsAsync())
            .ReturnsAsync(storyIds);
        _mockHackerNewsService.Setup(x => x.GetStoriesAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(stories);

        // Act
        var result = await _storyService.SearchStoriesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Stories.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}
