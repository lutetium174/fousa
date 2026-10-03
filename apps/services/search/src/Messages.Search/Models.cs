using Core;
﻿namespace Messages.Search;

// ============================================================================
// DSL Types
// ============================================================================

/// <summary>
/// Time range options for DSL parsing
/// </summary>
public class TimeRange
{
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public string? RelativePeriod { get; set; }
    
    public static TimeRange Parse(string value)
    {
        value = value.Trim();
        
        // Check for date range: 2024-01-15..2024-01-20
        if (value.Contains(".."))
        {
            var parts = value.Split([".."], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return new()
                {
                    Start = ParseDate(parts[0].Trim()),
                    End = ParseDate(parts[1].Trim())
                };
            }
        }
        
        // Check for comparison operators
        if (value.StartsWith(">=") && value.Length > 2)
        {
            return new() { Start = ParseDate(value.Substring(2).Trim()), End = null };
        }
        if (value.StartsWith(">") && value.Length > 1)
        {
            return new() { Start = ParseDate(value.Substring(1).Trim()), End = null };
        }
        if (value.StartsWith("<=") && value.Length > 2)
        {
            return new() { Start = null, End = ParseDate(value.Substring(2).Trim()) };
        }
        if (value.StartsWith("<") && value.Length > 1)
        {
            return new() { Start = null, End = ParseDate(value.Substring(1).Trim()) };
        }
        
        // Check for exact date
        if (DateTimeOffset.TryParse(value, out var exactDate))
        {
            return new() { Start = exactDate, End = exactDate.AddDays(1) };
        }
        
        // Check for year-month
        if (value is [_, _, _, _, '-', _, _])
        {
            if (int.TryParse(value[..4], out var year) && 
                int.TryParse(value.AsSpan(5, 2), out var month))
            {
                return new TimeRange
                {
                    Start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero),
                    End = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1)
                };
            }
        }
        
        // Relative date
        return new TimeRange { RelativePeriod = value };
    }
    
    private static DateTimeOffset? ParseDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;
        if (DateTimeOffset.TryParse(dateStr, out var result))
            return result;
        return null;
    }
}

/// <summary>
/// Size filter for messages
/// </summary>
public class SizeFilter
{
    public long? Bytes { get; set; }
    public SizeComparison Comparison { get; set; }
    
    public static SizeFilter Parse(string value)
    {
        var comparison = SizeComparison.Equal;
        var sizeStr = value;
        
        if (sizeStr.StartsWith(">="))
        {
            comparison = SizeComparison.GreaterThanOrEqual;
            sizeStr = sizeStr.Substring(2);
        }
        else if (sizeStr.StartsWith(">"))
        {
            comparison = SizeComparison.GreaterThan;
            sizeStr = sizeStr.Substring(1);
        }
        else if (sizeStr.StartsWith("<="))
        {
            comparison = SizeComparison.LessThanOrEqual;
            sizeStr = sizeStr.Substring(2);
        }
        else if (sizeStr.StartsWith("<"))
        {
            comparison = SizeComparison.LessThan;
            sizeStr = sizeStr.Substring(1);
        }
        
        var bytes = ParseSize(sizeStr);
        return new SizeFilter { Bytes = bytes, Comparison = comparison };
    }
    
    private static long? ParseSize(string sizeStr)
    {
        sizeStr = sizeStr.Trim().ToLower();
        
        if (long.TryParse(sizeStr, out var bytes))
            return bytes;
        
        var units = new Dictionary<string, long>
        {
            ["b"] = 1,
            ["kb"] = 1024,
            ["mb"] = 1024 * 1024,
            ["gb"] = 1024L * 1024 * 1024
        };
        
        foreach (var unit in units)
        {
            if (sizeStr.EndsWith(unit.Key))
            {
                var numStr = sizeStr.Substring(0, sizeStr.Length - unit.Key.Length);
                if (double.TryParse(numStr, out var num))
                    return (long)(num * unit.Value);
            }
        }
        
        return null;
    }
}