using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Kafka.LinqToKafka.Extensions;

namespace Kafka.LinqToKafka;

public class KafkaQueryProvider(HttpClient httpClient, string baseUrl) : IAsyncQueryProvider
{
    private readonly HttpClient? _httpClient = httpClient;
    private readonly string? _baseUrl = baseUrl;

    public IQueryable CreateQuery(Expression expression)
        => (IQueryable)typeof(KafkaQueryable<>)
            .MakeGenericType(expression.Type.GetSequenceElementType())
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [typeof(KafkaQueryProvider), typeof(Expression)],
                null)
            !.Invoke([this, expression]);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new KafkaQueryable<TElement>(this, expression);

    public object Execute(Expression expression)
        => Execute<object>(expression);

    public TResult Execute<TResult>(Expression expression)
        => ExecuteAsync<TResult>(expression).Result;

    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => _httpClient != null && _baseUrl != null
            ? ExecutePinotQueryAsync<TResult>(expression, cancellationToken)
            : throw new InvalidOperationException("No query source configured");

    private async Task<TResult> ExecutePinotQueryAsync<TResult>(Expression expression,
        CancellationToken cancellationToken)
    {
        var json = await ExecuteQuery(expression, cancellationToken);

        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("resultTable", out var resultTable) ||
            !resultTable.TryGetProperty("dataSchema", out var dataSchema) ||
            !resultTable.TryGetProperty("rows", out var rows))
        {
            throw new JsonException("Invalid Pinot query response format");
        }

        var columnNames = dataSchema
            .GetProperty("columnNames")
            .EnumerateArray()
            .Select(x => x.GetString()!)
            .ToArray();

        var elementType = typeof(TResult).GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var row in rows.EnumerateArray())
        {
            if (cancellationToken.IsCancellationRequested) break;

            var item = Activator.CreateInstance(elementType)!;
            MapRowToObject(row, columnNames, elementType, item);
            list.Add(item);
        }

        return (TResult)list;
    }

    private async Task<string> ExecuteQuery(Expression expression, CancellationToken cancellationToken)
    {
        var visitor = new KafkaExpressionVisitor();
        visitor.Visit(expression);

        var content = new StringContent(
            JsonSerializer.Serialize(new { sql = visitor.Query.Trim() }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient!.PostAsync(
            $"{_baseUrl}/sql",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static void MapRowToObject(JsonElement row, string[] columnNames, Type targetType, object target)
    {
        var properties = targetType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(
                p => p.Name,
                p => p, StringComparer.OrdinalIgnoreCase);

        var rowValues = row.EnumerateArray().ToArray();

        for (var i = 0; i < columnNames.Length && i < rowValues.Length; i++)
        {
            if (!properties.TryGetValue(columnNames[i], out var property) || !property.CanWrite) continue;
            property.SetValue(target, ConvertValue(rowValues[i], property.PropertyType));
        }
    }

    private static object? ConvertValue(JsonElement element, Type targetType)
        => element.ValueKind switch
        {
            JsonValueKind.Number when targetType == typeof(int) => element.GetInt32(),
            JsonValueKind.Number when targetType == typeof(long) => element.GetInt64(),
            JsonValueKind.Number when targetType == typeof(double) => element.GetDouble(),
            JsonValueKind.Number when targetType == typeof(float) => element.GetSingle(),
            JsonValueKind.Number when targetType == typeof(decimal) => element.GetDecimal(),
            JsonValueKind.Number when targetType == typeof(short) => element.GetInt16(),

            JsonValueKind.True when targetType == typeof(bool) => true,
            JsonValueKind.False when targetType == typeof(bool) => false,

            JsonValueKind.String when targetType == typeof(string) => element.GetString(),
            JsonValueKind.String when targetType == typeof(DateTime) || targetType == typeof(DateTime?)
                => DateTime.Parse(element.GetString()!),
            JsonValueKind.String when targetType == typeof(Guid) || targetType == typeof(Guid?)
                => Guid.Parse(element.GetString()!),
            JsonValueKind.String when targetType == typeof(DateOnly) || targetType == typeof(DateOnly?)
                => DateOnly.Parse(element.GetString()!),
            JsonValueKind.String when targetType == typeof(TimeOnly) || targetType == typeof(TimeOnly?)
                => TimeOnly.Parse(element.GetString()!),
            JsonValueKind.String when targetType.IsEnum => Enum.Parse(targetType, element.GetString()!),
            JsonValueKind.String when targetType == typeof(byte) => element.GetByte(),

            JsonValueKind.Null => null,

            _ => Convert.ChangeType(element.GetRawText(), targetType)
        };
}