using System.Linq.Expressions;
using Core;

namespace Messages.Search.Models;

/// <summary>
/// Represents a parsed query with all its components
/// </summary>
public class ParsedQuery : IMessageQuery
{
    public string OriginalQuery { get; set; } = string.Empty;
    public List<QueryToken> Tokens { get; set; } = [];
    public List<string> FreeTextTerms { get; set; } = [];
    public List<string> Phrases { get; set; } = [];
    
    // Parsed filter values
    public List<string> FromParticipants { get; set; } = [];
    public List<string> ToParticipants { get; set; } = [];
    public List<string> InvolvingParticipants { get; set; } = [];
    public List<string> Channels { get; set; } = [];
    public List<MessageStatus> Statuses { get; set; } = [];
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
    public List<(MessageSortField Field, SortDirection Direction)> SortFields { get; } = [];
    public Expression<Func<Message, bool>>? Predicate { get; }
    public List<(Expression<Func<Message, object>> KeySelector, SortDirection Direction)> SortExpressions { get; }
    public Expression<Func<Message, object>>? Selector { get; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string[] Includes { get; }
    public IMessageQuery And(IMessageQuery other)
    {
        throw new NotImplementedException();
    }

    public IMessageQuery Or(IMessageQuery other)
    {
        throw new NotImplementedException();
    }

    public IMessageQuery Not()
    {
        throw new NotImplementedException();
    }

    public IQueryable<Message> ApplyTo(IQueryable<Message> source)
    {
        throw new NotImplementedException();
    }

    // Boolean structure (for complex queries with AND/OR/NOT)
    public List<QueryClause> Clauses { get; } = [];
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
}