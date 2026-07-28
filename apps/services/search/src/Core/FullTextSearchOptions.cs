namespace Core;

public class FullTextSearchOptions
{
    public string? Language { get; set; }
    public bool UseFuzzyMatching { get; set; }
    public int Fuzziness { get; set; } = 2;
    public string[]? SearchFields { get; set; }
}