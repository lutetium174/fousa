namespace Messages.Search;

/// <summary>
/// Represents a boolean clause in the query (AND/OR/NOT grouping)
/// </summary>
public class QueryClause
{
    public BooleanOperator Operator { get; set; }
    public List<QueryToken> Tokens { get; } = new();
    public List<QueryClause> SubClauses { get; } = new();
    public bool IsNegated { get; set; }
}