using System.Text.Json;
using Messages.Kafka.Models;

namespace Messages.Kafka.Tests;

public class MessageDeserialiseTests
{
    [Fact]
    public Task Should() => Test.Run(services =>
        {
            var result = JsonSerializer.Deserialize<QueryResponse>(
                $"{{\"resultTable\": {{\"dataSchema\": {{\"columnNames\": [\"Content\", \"Id\", \"RoutingKey\", \"Sender\", \"Timestamp\"],\"columnDataTypes\": [\"STRING\", \"STRING\", \"STRING\", \"STRING\", \"TIMESTAMP\"]}},\"rows\": [[\"allo guvner\",\"f2051c49-4ccc-448d-8e3e-9d2b87de4af7\",\"default\",\"7655d018-fe75-4706-a0db-0769c44fa206\",\"2026-07-03 13:57:05.223\"]]}},\"numRowsResultSet\": 1,\"partialResult\": false,\"exceptions\": []}}",
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            Assert.NotEmpty(result.ResultTable.Rows);

            return Task.CompletedTask;
        },
        services => services);
}