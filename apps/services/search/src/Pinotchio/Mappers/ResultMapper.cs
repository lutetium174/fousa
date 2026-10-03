using System.Reflection;
using System.Text.Json;

namespace Messages.Search.Mappers;

public static class PinotResultMapper
{
    public static IEnumerable<T> Map<T>(string json)
    {
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        if (!root.TryGetProperty("resultTable", out var resultTable))
            throw new InvalidOperationException("Pinot response missing resultTable.");

        var columns = resultTable.GetProperty("dataSchema")
                                 .GetProperty("columnNames")
                                 .EnumerateArray()
                                 .Select(c => c.GetString())
                                 .ToArray();

        var rows = resultTable.GetProperty("rows").EnumerateArray();

        foreach (var row in rows)
        {
            yield return MapRow<T>(columns, row);
        }
    }

    private static T MapRow<T>(string?[] columns, JsonElement row)
    {
        var obj = Activator.CreateInstance<T>();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var values = row.EnumerateArray().ToArray();

        for (var i = 0; i < columns.Length; i++)
        {
            var colName = columns[i];
            var value = values[i];

            var prop = props.FirstOrDefault(p =>
                string.Equals(p.Name, colName, StringComparison.OrdinalIgnoreCase));

            if (prop == null || !prop.CanWrite)
                continue;

            var converted = ConvertValue(value, prop.PropertyType);
            prop.SetValue(obj, converted);
        }

        return obj;
    }

    private static object? ConvertValue(JsonElement element, Type targetType)
        => (element, targetType) switch 
        {
            ({ValueKind : JsonValueKind.Null}, _) => element.GetString(),
            
            _ when targetType == typeof(string) => element.GetString(),
            _ when targetType == typeof(int) => element.GetInt32(),
            _ when targetType == typeof(long) => element.GetInt64(),
            _ when targetType == typeof(double) => element.GetDouble(),
            _ when targetType == typeof(bool) => element.GetBoolean(),
            _ when targetType == typeof(DateTime) => DateTime.Parse(element.GetString()!),
            
            _ => JsonSerializer.Deserialize(element.GetRawText(), targetType)
        };
}
