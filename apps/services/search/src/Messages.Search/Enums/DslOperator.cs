namespace Messages.Search;

/// <summary>
/// Represents a parsed query operator (e.g., from:, during:, has:)
/// </summary>
public enum DslOperator
{
    None,
    // Text search
    Text,
    ExactPhrase,
    Wildcard,
    
    // Participants
    From,
    To,
    Involving,
    InvolvingAny,
    InvolvingAll,
    Cc,
    Bcc,
    
    // Channels
    In,
    Channel,
    
    // Temporal
    Created,
    CreatedBefore,
    CreatedAfter,
    Modified,
    ModifiedBefore,
    ModifiedAfter,
    During,
    
    // Metadata
    Status,
    Tag,
    Tags,
    AnyTag,
    AllTags,
    Has,
    Pinned,
    Size,
    
    // Sorting & Pagination
    Sort,
    Limit,
    Offset,
    Page,
    PageSize
}