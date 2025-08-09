using Microsoft.AspNetCore.Mvc;
using HackerNews.Backend.Models;
using HackerNews.Backend.Services;

namespace HackerNews.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _storyService;
    private readonly ILogger<StoriesController> _logger;

    public StoriesController(IStoryService storyService, ILogger<StoriesController> logger)
    {
        _storyService = storyService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the newest stories from Hacker News with optional search and pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="search">Optional search term to filter stories</param>
    /// <returns>Paginated list of newest stories</returns>
    [HttpGet("newest")]
    public async Task<ActionResult<ApiResponse<PaginatedStories>>> GetNewestStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        try
        {
            var parameters = new SearchParameters
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100), // Limit page size to 100
                Search = search
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaginatedStories>.ErrorResponse(
                    new PaginatedStories(), 
                    "Invalid parameters provided"));
            }

            var result = await _storyService.GetNewestStoriesAsync(parameters);
            
            return Ok(ApiResponse<PaginatedStories>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting newest stories");
            return StatusCode(500, ApiResponse<PaginatedStories>.ErrorResponse(
                new PaginatedStories(), 
                "An error occurred while fetching stories"));
        }
    }

    /// <summary>
    /// Searches stories based on query with pagination
    /// </summary>
    /// <param name="query">Search query to filter stories by title, content, or author</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of matching stories</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedStories>>> SearchStories(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var parameters = new SearchParameters
            {
                Query = query,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PaginatedStories>.ErrorResponse(
                    new PaginatedStories(), 
                    "Invalid parameters provided"));
            }

            var result = await _storyService.SearchStoriesAsync(parameters);
            
            return Ok(ApiResponse<PaginatedStories>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stories");
            return StatusCode(500, ApiResponse<PaginatedStories>.ErrorResponse(
                new PaginatedStories(), 
                "An error occurred while searching stories"));
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>API health status</returns>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
