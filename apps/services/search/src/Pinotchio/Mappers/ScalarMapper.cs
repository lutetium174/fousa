using System.Text.Json;

namespace Messages.Search.Mappers;

public static class PinotScalarMapper
{
    public static T Map<T>(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("resultTable", out var resultTable))
            throw new InvalidOperationException("Pinot response missing resultTable.");

        var rows = resultTable.GetProperty("rows").EnumerateArray();

        var firstRow = rows.FirstOrDefault();
        if (firstRow.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException("Pinot response has no rows.");

        var firstValue = firstRow.EnumerateArray().FirstOrDefault();
        if (firstValue.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException("Pinot response row has no columns.");

        return (T)ConvertValue(firstValue, typeof(T))!;
    }

    private static object? ConvertValue(JsonElement element, Type targetType)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return null;

        if (targetType == typeof(string))
            return element.GetString();

        if (targetType == typeof(int))
            return element.GetInt32();

        if (targetType == typeof(long))
            return element.GetInt64();

        if (targetType == typeof(double))
            return element.GetDouble();

        if (targetType == typeof(bool))
            return element.GetBoolean();

        if (targetType == typeof(DateTime))
            return DateTime.Parse(element.GetString()!);

        return JsonSerializer.Deserialize(element.GetRawText(), targetType);
    }
}
