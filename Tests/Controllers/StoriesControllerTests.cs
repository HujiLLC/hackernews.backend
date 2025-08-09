using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using HackerNews.Backend.Controllers;
using HackerNews.Backend.Services;
using HackerNews.Backend.Models;

namespace HackerNews.Backend.Tests.Controllers;

public class StoriesControllerTests
{
    private readonly Mock<IStoryService> _mockStoryService;
    private readonly Mock<ILogger<StoriesController>> _mockLogger;
    private readonly StoriesController _controller;

    public StoriesControllerTests()
    {
        _mockStoryService = new Mock<IStoryService>();
        _mockLogger = new Mock<ILogger<StoriesController>>();
        _controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetNewestStories_ShouldReturnOkResult_WhenValidParameters()
    {
        // Arrange
        var expectedStories = new PaginatedStories
        {
            Stories = new List<Story>
            {
                new() { Id = 1, Title = "Test Story", Score = 100, By = "user1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
            TotalPages = 1
        };

        _mockStoryService.Setup(x => x.GetNewestStoriesAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(expectedStories);

        // Act
        var result = await _controller.GetNewestStories();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PaginatedStories>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedStories);
    }

    [Fact]
    public async Task GetNewestStories_ShouldLimitPageSize_WhenPageSizeExceedsMaximum()
    {
        // Arrange
        var expectedStories = new PaginatedStories();
        _mockStoryService.Setup(x => x.GetNewestStoriesAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(expectedStories);

        // Act
        await _controller.GetNewestStories(pageSize: 150);

        // Assert
        _mockStoryService.Verify(x => x.GetNewestStoriesAsync(
            It.Is<SearchParameters>(p => p.PageSize == 100)), Times.Once);
    }

    [Fact]
    public async Task GetNewestStories_ShouldHandleSearchParameter_WhenProvided()
    {
        // Arrange
        var searchTerm = "angular";
        var expectedStories = new PaginatedStories();
        _mockStoryService.Setup(x => x.GetNewestStoriesAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(expectedStories);

        // Act
        await _controller.GetNewestStories(search: searchTerm);

        // Assert
        _mockStoryService.Verify(x => x.GetNewestStoriesAsync(
            It.Is<SearchParameters>(p => p.Search == searchTerm)), Times.Once);
    }

    [Fact]
    public async Task SearchStories_ShouldReturnOkResult_WhenValidQuery()
    {
        // Arrange
        var query = "javascript";
        var expectedStories = new PaginatedStories
        {
            Stories = new List<Story>
            {
                new() { Id = 1, Title = "JavaScript Tutorial", Score = 150, By = "dev1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
            TotalPages = 1
        };

        _mockStoryService.Setup(x => x.SearchStoriesAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(expectedStories);

        // Act
        var result = await _controller.SearchStories(query);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PaginatedStories>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedStories);
    }

    [Fact]
    public async Task SearchStories_ShouldCallSearchService_WithCorrectParameters()
    {
        // Arrange
        var query = "react";
        var page = 2;
        var pageSize = 10;
        var expectedStories = new PaginatedStories();

        _mockStoryService.Setup(x => x.SearchStoriesAsync(It.IsAny<SearchParameters>()))
            .ReturnsAsync(expectedStories);

        // Act
        await _controller.SearchStories(query, page, pageSize);

        // Assert
        _mockStoryService.Verify(x => x.SearchStoriesAsync(
            It.Is<SearchParameters>(p => 
                p.Query == query && 
                p.Page == page && 
                p.PageSize == pageSize)), Times.Once);
    }

    [Fact]
    public async Task GetNewestStories_ShouldReturnInternalServerError_WhenServiceThrows()
    {
        // Arrange
        _mockStoryService.Setup(x => x.GetNewestStoriesAsync(It.IsAny<SearchParameters>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetNewestStories();

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        var response = statusResult.Value.Should().BeOfType<ApiResponse<PaginatedStories>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("error occurred");
    }

    [Fact]
    public async Task SearchStories_ShouldReturnInternalServerError_WhenServiceThrows()
    {
        // Arrange
        _mockStoryService.Setup(x => x.SearchStoriesAsync(It.IsAny<SearchParameters>()))
            .ThrowsAsync(new Exception("Search service error"));

        // Act
        var result = await _controller.SearchStories("test");

        // Assert
        result.Should().NotBeNull();
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        var response = statusResult.Value.Should().BeOfType<ApiResponse<PaginatedStories>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("error occurred");
    }

    [Fact]
    public void GetHealth_ShouldReturnOkResult_WithHealthStatus()
    {
        // Act
        var result = _controller.GetHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().NotBeNull();
    }
}
