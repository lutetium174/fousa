namespace Messages.Search;

/// <summary>
/// Represents a token in the query string
/// </summary>
public class QueryToken
{
    public DslOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsNegated { get; set; }
    public bool IsPhrase { get; set; }
    public bool IsWildcard { get; set; }
    public int Position { get; set; }
    
    public override string ToString()
    {
        var op = Operator != DslOperator.None ? Operator.ToString().ToLower() : "";
        var prefix = IsNegated ? "-" : string.Empty;
        var val = Value;
        if (IsPhrase) val = $"\"{val}\"";
        if (IsWildcard) val = $"*{val}*";
        return string.IsNullOrEmpty(op) ? $"{prefix}{val}" : $"{prefix}{op}:{val}";
    }
}