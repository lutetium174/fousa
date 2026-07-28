using Core;

namespace Messages.Search;

/// <summary>
/// API request model for search
/// </summary>
public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public List<string>? Include { get; set; }
    public FullTextSearchOptions? FullTextOptions { get; set; }
}