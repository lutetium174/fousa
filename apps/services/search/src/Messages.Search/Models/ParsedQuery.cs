using Core;

namespace Messages.Search;

/// <summary>
/// Represents a parsed query with all its components
/// </summary>
public class ParsedQuery : IMessageQuery
{
    public string OriginalQuery { get; set; } = string.Empty;
    public List<QueryToken> Tokens { get; set; } = new();
    public List<string> FreeTextTerms { get; set; } = new();
    public List<string> Phrases { get; set; } = new();
    
    // Parsed filter values
    public List<string> FromParticipants { get; set; } = new();
    public List<string> ToParticipants { get; set; } = new();
    public List<string> InvolvingParticipants { get; set; } = new();
    public List<string> Channels { get; set; } = new();
    public List<MessageStatus> Statuses { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> AnyTags { get; set; } = new();
    public List<string> AllTags { get; set; } = new();
    public List<string> HasFeatures { get; set; } = new();
    public List<string> ExcludedHasFeatures { get; set; } = new();
    public TimeRange? CreatedRange { get; set; }
    public TimeRange? ModifiedRange { get; set; }
    public bool? IsPinned { get; set; }
    public SizeFilter? SizeFilter { get; set; }
    
    // Sorting and pagination
    public MessageSortField? SortField { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    
    // Boolean structure (for complex queries with AND/OR/NOT)
    public List<QueryClause> Clauses { get; } = new();
}