using System.ComponentModel.DataAnnotations;

namespace HackerNews.Backend.Models;

public class SearchParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public string? Query { get; set; }
}
