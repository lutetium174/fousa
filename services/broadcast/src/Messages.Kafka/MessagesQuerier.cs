using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Core;
using Core.Broker;
using Foundation;
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
    
    public async Task<Result<IEnumerable<string>>> Lookup(MessagesFilter filter, CancellationToken cancellationToken)
    {
        var effectiveLimit = filter.Limit ?? 1000; // Prevent unbounded result sets

        // If Pinot is configured, use Pinot for lookup
        if (_pinotOptions != null && !string.IsNullOrEmpty(_pinotOptions.ControllerUri))
        {
            return await QueryPinotWithLinq(filter, effectiveLimit, cancellationToken);
        }

        // Fallback to Kafka consumer for lookup
        var topic = _kafkaOptions.Topic ?? "messages";
        var results = new List<string>();
        await QueryKafka(topic, results, filter, effectiveLimit, cancellationToken);

        return new(results);
    }

    private async Task QueryKafka(string topic, List<string> results, MessagesFilter filter, int limit,
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

            // Use Consume with timeout - will return null if no message available
            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(2));
            if (consumeResult == null)
            {
                // No more messages available - this might mean we've reached the end
                noMessageCount++;
                continue;
            }

            noMessageCount = 0; // Reset counter when we get a message
            var json = Encoding.UTF8.GetString(consumeResult.Message.Value);
            var messageModel = JsonSerializer.Deserialize<MessageModel>(json);

            if (messageModel == null || !PassesFilter(messageModel, filter))
                continue;

            results.Add(messageModel.Content);
            messageCount++;
        }
    }

    private async Task<Result<IEnumerable<string>>> QueryPinotWithLinq(
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

        // Parse the Pinot response
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var pinotResponse = JsonSerializer.Deserialize<QueryResponse>(
            responseContent,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        if (pinotResponse?.ResultTable == null) return new(null, new("No results found"));


        // Extract message contents and apply additional filtering with LINQ
        var messages = new List<string>();

        // Process the results from Pinot - using LINQ to process results
        var contentResults = pinotResponse.ResultTable.Rows.Select(row => row[0].ToString());

        messages.AddRange(contentResults);

        // Apply additional LINQ filtering if needed
        if (!filter.IsEmpty)
        {
            messages = messages
                .Where(content => string.IsNullOrEmpty(filter.ContentContains) ||
                                  content.Contains(filter.ContentContains, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();
        }

        return new(messages.Take(limit));
    }

    private string BuildPinotSqlQuery(MessagesFilter filter, int limit)
    {
        var tableName = _pinotOptions?.TableName ?? "messages";
        var whereClause = BuildPinotWhereClause(filter);

        // Build SQL query - using LINQ-like syntax
        var query = $"SELECT * FROM {tableName}";

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