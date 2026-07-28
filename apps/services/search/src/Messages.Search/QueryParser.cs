using System.Globalization;
using System.Text.RegularExpressions;
using Core;

namespace Messages.Search;

public class QueryParser
{
    private static readonly Dictionary<string, DslOperator> OperatorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["from"] = DslOperator.From,
        ["to"] = DslOperator.To,
        ["involving"] = DslOperator.Involving,
        ["involvingany"] = DslOperator.InvolvingAny,
        ["involvingall"] = DslOperator.InvolvingAll,
        ["cc"] = DslOperator.Cc,
        ["bcc"] = DslOperator.Bcc,
        ["in"] = DslOperator.In,
        ["channel"] = DslOperator.Channel,
        ["created"] = DslOperator.Created,
        ["createdbefore"] = DslOperator.CreatedBefore,
        ["createdafter"] = DslOperator.CreatedAfter,
        ["modified"] = DslOperator.Modified,
        ["modifiedbefore"] = DslOperator.ModifiedBefore,
        ["modifiedafter"] = DslOperator.ModifiedAfter,
        ["during"] = DslOperator.During,
        ["status"] = DslOperator.Status,
        ["tag"] = DslOperator.Tag,
        ["tags"] = DslOperator.Tags,
        ["anytag"] = DslOperator.AnyTag,
        ["alltags"] = DslOperator.AllTags,
        ["has"] = DslOperator.Has,
        ["pinned"] = DslOperator.Pinned,
        ["size"] = DslOperator.Size,
        ["sort"] = DslOperator.Sort,
        ["limit"] = DslOperator.Limit,
        ["offset"] = DslOperator.Offset,
        ["page"] = DslOperator.Page,
        ["pagesize"] = DslOperator.PageSize
    };

    private static readonly Dictionary<string, MessageStatus> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = MessageStatus.Draft,
        ["sent"] = MessageStatus.Sent,
        ["delivered"] = MessageStatus.Delivered,
        ["read"] = MessageStatus.Read,
        ["archived"] = MessageStatus.Archived,
        ["deleted"] = MessageStatus.Deleted
    };

    private static readonly Dictionary<string, MessageSortField> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdat"] = MessageSortField.CreatedAt,
        ["created"] = MessageSortField.CreatedAt,
        ["modifiedat"] = MessageSortField.ModifiedAt,
        ["modified"] = MessageSortField.ModifiedAt,
        ["sender"] = MessageSortField.Sender,
        ["subject"] = MessageSortField.Subject,
        ["importance"] = MessageSortField.Importance
    };

    private static readonly Dictionary<string, SortDirection> SortDirectionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["asc"] = SortDirection.Ascending,
        ["ascending"] = SortDirection.Ascending,
        ["desc"] = SortDirection.Descending,
        ["descending"] = SortDirection.Descending
    };

    public ParsedQuery Parse(string query)
    {
        var parsed = new ParsedQuery { OriginalQuery = query };
        if (string.IsNullOrWhiteSpace(query))
            return parsed;

        query = NormalizeQuery(query);
        ParseTokens(query, parsed);
        ApplyParsedValues(parsed);
        return parsed;
    }

    private string NormalizeQuery(string query)
    {
        query = query.Trim();
        query = Regex.Replace(query, @"\s+", " ");
        return query;
    }

    private void ParseTokens(string query, ParsedQuery parsed)
    {
        var tokens = SplitByWhitespace(query);
        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;
            var parsedToken = ParseToken(token);
            if (parsedToken != null)
                parsed.Tokens.Add(parsedToken);
        }
    }

    private List<string> SplitByWhitespace(string input)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }
        if (current.Length > 0)
            result.Add(current.ToString());
        return result;
    }

    private QueryToken? ParseToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var parsedToken = new QueryToken { Position = 0 };

        if (token.StartsWith("-"))
        {
            parsedToken.IsNegated = true;
            token = token.Substring(1);
            parsedToken.Value = token;
        }
        else
        {
            parsedToken.Value = token;
        }

        if (token.StartsWith('"') && token.EndsWith('"') && token.Length > 1)
        {
            parsedToken.IsPhrase = true;
            parsedToken.Value = token.Substring(1, token.Length - 2);
            parsedToken.Operator = DslOperator.ExactPhrase;
            return parsedToken;
        }

        if (token.Contains('*') || token.Contains('?'))
        {
            parsedToken.IsWildcard = true;
            parsedToken.Operator = DslOperator.Wildcard;
            return parsedToken;
        }

        var colonIndex = token.IndexOf(':');
        if (colonIndex > 0)
        {
            var opStr = token.Substring(0, colonIndex);
            var value = token.Substring(colonIndex + 1);
            if (OperatorMap.TryGetValue(opStr, out var op))
            {
                parsedToken.Operator = op;
                parsedToken.Value = value;
                return parsedToken;
            }
        }
        else if (OperatorMap.TryGetValue(token, out var opWithoutValue))
        {
            // Handle operators without values (like "pinned" without ":true")
            parsedToken.Operator = opWithoutValue;
            parsedToken.Value = string.Empty;
            return parsedToken;
        }

        parsedToken.Operator = DslOperator.Text;
        return parsedToken;
    }

    private void ApplyParsedValues(ParsedQuery parsed)
    {
        foreach (var token in parsed.Tokens)
        {
            if (token.Operator == DslOperator.None || string.IsNullOrWhiteSpace(token.Value))
                continue;
            ApplyTokenValue(parsed, token);
        }
    }

    private void ApplyTokenValue(ParsedQuery parsed, QueryToken token)
    {
        switch (token.Operator)
        {
            case DslOperator.Text:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.FreeTextTerms.Add(token.Value);
                break;
            case DslOperator.ExactPhrase:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.Phrases.Add(token.Value);
                break;
            case DslOperator.Wildcard:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.FreeTextTerms.Add(token.Value);
                break;
            case DslOperator.From:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.FromParticipants.Add(token.Value);
                break;
            case DslOperator.To:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.ToParticipants.Add(token.Value);
                break;
            case DslOperator.Involving:
            case DslOperator.InvolvingAny:
            case DslOperator.InvolvingAll:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var ids = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in ids)
                        parsed.InvolvingParticipants.Add(id.Trim());
                }
                break;
            case DslOperator.Cc:
            case DslOperator.Bcc:
                break;
            case DslOperator.In:
            case DslOperator.Channel:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var channels = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var channel in channels)
                        parsed.Channels.Add(channel.Trim());
                }
                break;
            case DslOperator.Created:
            case DslOperator.During:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.CreatedRange = TimeRange.Parse(token.Value);
                break;
            case DslOperator.CreatedBefore:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var endDate = TimeRange.Parse(token.Value);
                    parsed.CreatedRange = new TimeRange { End = endDate.End };
                }
                break;
            case DslOperator.CreatedAfter:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var startDate = TimeRange.Parse(token.Value);
                    parsed.CreatedRange = new TimeRange { Start = startDate.Start };
                }
                break;
            case DslOperator.Modified:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.ModifiedRange = TimeRange.Parse(token.Value);
                break;
            case DslOperator.ModifiedBefore:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var endDate = TimeRange.Parse(token.Value);
                    parsed.ModifiedRange = new TimeRange { End = endDate.End };
                }
                break;
            case DslOperator.ModifiedAfter:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var startDate = TimeRange.Parse(token.Value);
                    parsed.ModifiedRange = new TimeRange { Start = startDate.Start };
                }
                break;
            case DslOperator.Status:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var statuses = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var statusStr in statuses)
                    {
                        if (StatusMap.TryGetValue(statusStr.Trim(), out var status))
                            parsed.Statuses.Add(status);
                    }
                }
                break;
            case DslOperator.Tag:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.Tags.Add(token.Value);
                break;
            case DslOperator.Tags:
            case DslOperator.AnyTag:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var tags = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                        parsed.AnyTags.Add(tag.Trim());
                }
                break;
            case DslOperator.AllTags:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var tags = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                        parsed.AllTags.Add(tag.Trim());
                }
                break;
            case DslOperator.Has:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var features = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var feature in features)
                    {
                        if (token.IsNegated)
                            parsed.ExcludedHasFeatures.Add(feature.Trim());
                        else
                            parsed.HasFeatures.Add(feature.Trim());
                    }
                }
                break;
            case DslOperator.Pinned:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    if (bool.TryParse(token.Value, out var isPinned))
                        parsed.IsPinned = isPinned;
                    else if (token.Value.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                             token.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        parsed.IsPinned = bool.Parse(token.Value);
                    else
                        parsed.IsPinned = true; // If just "pinned" without value or with non-boolean value
                }
                else
                {
                    parsed.IsPinned = true; // If just "pinned" without value
                }
                break;
            case DslOperator.Size:
                if (!string.IsNullOrWhiteSpace(token.Value))
                    parsed.SizeFilter = SizeFilter.Parse(token.Value);
                break;
            case DslOperator.Sort:
                if (!string.IsNullOrWhiteSpace(token.Value))
                {
                    var sortParts = token.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (sortParts.Length > 0)
                    {
                        if (SortFieldMap.TryGetValue(sortParts[0].Trim(), out var field))
                            parsed.SortField = field;
                        if (sortParts.Length > 1 && SortDirectionMap.TryGetValue(sortParts[1].Trim(), out var direction))
                            parsed.SortDirection = direction;
                    }
                }
                break;
            case DslOperator.Limit:
                if (int.TryParse(token.Value, out var limit))
                    parsed.Limit = limit;
                break;
            case DslOperator.Offset:
                if (int.TryParse(token.Value, out var offset))
                    parsed.Offset = offset;
                break;
            case DslOperator.Page:
                if (int.TryParse(token.Value, out var page))
                    parsed.Page = page;
                break;
            case DslOperator.PageSize:
                if (int.TryParse(token.Value, out var pageSize))
                    parsed.PageSize = pageSize;
                break;
        }
    }
}
