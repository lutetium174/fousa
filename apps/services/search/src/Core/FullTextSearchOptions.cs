namespace Core;

public class FullTextSearchOptions
{
    public string? Language { get; set; }
    public bool UseFuzzyMatching { get; set; }
    public int Fuzziness { get; set; } = 2;
    public string[]? SearchFields { get; set; }
    public MessageSortField SortField { get; set; }
    public SortDirection SortDirection { get; set; }
    public int? PageSize { get; set; }
    public int? Page { get; set; }
    public int? Limit { get; set; }
}