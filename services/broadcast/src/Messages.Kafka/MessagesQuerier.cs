using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Core;
using Core.Broker;
using Core.Responses;
using Foundation;
using Messages.Kafka.JsonExtensions;
using Messages.Kafka.Models;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

public class MessagesQuerier(
    IOptions<KafkaOptions> kafkaOptions,
    IOptions<PinotOptions> pinotOptions,
    HttpClient httpClient)
    : IMessagesQuerier, IDisposable
{
    private readonly KafkaOptions _kafkaOptions = kafkaOptions.Value;
    private readonly PinotOptions _pinotOptions = pinotOptions.Value;

    private IConsumer<byte[], byte[]> CreateConsumer()
    {
        var consumer = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = Guid.NewGuid().ToString(), // Unique group per lookup
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Don't commit offsets for lookup
            ClientId = _kafkaOptions.ClientId ?? "message-service-lookup"
        };

        if (string.IsNullOrEmpty(_kafkaOptions.Username) || string.IsNullOrEmpty(_kafkaOptions.Password))
            return new ConsumerBuilder<byte[], byte[]>(consumer).Build();

        consumer.SecurityProtocol = SecurityProtocol.SaslSsl;
        consumer.SaslMechanism = SaslMechanism.Plain;
        consumer.SaslUsername = _kafkaOptions.Username;
        consumer.SaslPassword = _kafkaOptions.Password;

        return new ConsumerBuilder<byte[], byte[]>(consumer).Build();
    }

    public async Task<Result<IEnumerable<MessageResponse>>> Lookup(MessagesFilter filter,
        CancellationToken cancellationToken)
    {
        var effectiveLimit = filter.Limit ?? 1000; // Prevent unbounded result sets

        // If Pinot is configured, use Pinot for lookup
        if (_pinotOptions != null && !string.IsNullOrEmpty(_pinotOptions.ControllerUri))
        {
            return await QueryPinotWithLinq(filter, effectiveLimit, cancellationToken);
        }

        // Fallback to Kafka consumer for lookup
        var topic = _kafkaOptions.Topic ?? "messages";
        var results = new List<MessageResponse>();
        await QueryKafka(topic, results, filter, effectiveLimit, cancellationToken);

        return new(results);
    }

    private Task QueryKafka(string topic, List<MessageResponse> results, MessagesFilter filter, int limit,
        CancellationToken cancellationToken)
    {
        using var consumer = CreateConsumer();
        consumer.Subscribe(topic);

        var messageCount = 0;
        var noMessageCount = 0;
        var maxNoMessageRetries = 5; // Stop if we get no messages 5 times in a row

        while (messageCount < limit && noMessageCount < maxNoMessageRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(2));
            if (consumeResult == null)
            {
                noMessageCount++;
                continue;
            }

            noMessageCount = 0;
            var json = Encoding.UTF8.GetString(consumeResult.Message.Value);
            var message = JsonSerializer.Deserialize<MessageModel>(json);

            if (message == null || !PassesFilter(message, filter))
                continue;

            results.Add(new(
                message.Id,
                message.Sender,
                message.Content,
                message.Timestamp
            ));
            messageCount++;
        }

        return Task.CompletedTask;
    }

    private async Task<Result<IEnumerable<MessageResponse>>> QueryPinotWithLinq(
        MessagesFilter filter,
        int limit,
        CancellationToken cancellationToken)
    {
        var queryUrl = BuildPinotQueryUrl();
        var response = await httpClient.PostAsJsonAsync(
            queryUrl,
            new
            {
                sql = BuildPinotSqlQuery(filter, limit)
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var pinotResponse = JsonSerializer.Deserialize<QueryResponse>(
            responseContent,
            MessagesQuerierJsonOptions.Options);

        if (pinotResponse?.ResultTable == null) return new(null, new("No results found"));

        var messages = new List<MessageResponse>(pinotResponse.ResultTable.Rows?.Select(Map) ?? []);

        if (!filter.IsEmpty)
        {
            messages = messages
                .Where(content => string.IsNullOrEmpty(filter.ContentContains) ||
                                  content.Message.Contains(filter.ContentContains, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();
        }

        return new(messages.Take(limit));
    }

    private MessageResponse Map(List<object> row) => new(
        Guid.Parse(row[1].ToString()!),
        Guid.Parse(row[3].ToString()!),
        row[0].ToString()!,
        DateTime.Parse(row[4].ToString()!));

    private string BuildPinotSqlQuery(MessagesFilter filter, int limit)
    {
        var cultureTable = filter.Culture is not null ? $"-{filter.Culture.Name}" : "";
        var tableName = $"{_pinotOptions.TableName ?? "messages"}{cultureTable}";
        var whereClause = BuildPinotWhereClause(filter);

        var query = $"SELECT * FROM \"{tableName}\"";

        if (!string.IsNullOrEmpty(whereClause))
        {
            query += " WHERE " + whereClause;
        }

        query += " LIMIT " + limit;

        return query;
    }

    private string BuildPinotWhereClause(MessagesFilter filter)
    {
        var conditions = new List<string>();

        // Add conditions using LINQ-like approach
        if (!string.IsNullOrEmpty(filter.RoutingKey))
        {
            conditions.Add($"routingKey = '{filter.RoutingKey.Replace("'", "''")}'");
        }

        if (!string.IsNullOrEmpty(filter.ContentContains))
        {
            conditions.Add($"content LIKE '%{filter.ContentContains.Replace("'", "''")}%'");
        }

        if (filter.FromDate.HasValue)
        {
            var fromDateEpoch = new DateTimeOffset(filter.FromDate.Value).ToUnixTimeMilliseconds();
            conditions.Add($"timestamp >= {fromDateEpoch}");
        }

        if (filter.ToDate.HasValue)
        {
            var toDateEpoch = new DateTimeOffset(filter.ToDate.Value).ToUnixTimeMilliseconds();
            conditions.Add($"timestamp <= {toDateEpoch}");
        }

        if (filter.Sender.HasValue)
        {
            conditions.Add($"sender = '{filter.Sender.Value}'");
        }

        return string.Join(" AND ", conditions);
    }

    private string BuildPinotQueryUrl()
        => string.IsNullOrEmpty(_pinotOptions.ControllerUri)
            ? throw new InvalidOperationException("Pinot controller URI is not configured")
            : $"{_pinotOptions.ControllerUri.TrimEnd('/')}/sql";

    private bool PassesFilter(MessageModel message, MessagesFilter filter)
    {
        if (filter.IsEmpty)
            return true;

        if (!string.IsNullOrEmpty(filter.RoutingKey) &&
            message.RoutingKey != filter.RoutingKey)
            return false;

        if (!string.IsNullOrEmpty(filter.ContentContains) &&
            !message.Content.Contains(filter.ContentContains, StringComparison.OrdinalIgnoreCase))
            return false;

        if (filter.FromDate.HasValue && message.Timestamp < filter.FromDate.Value)
            return false;

        if (filter.ToDate.HasValue && message.Timestamp > filter.ToDate.Value)
            return false;

        if (filter.Sender.HasValue && message.Sender != filter.Sender.Value)
            return false;

        return true;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}