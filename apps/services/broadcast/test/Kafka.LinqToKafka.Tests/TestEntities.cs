namespace Kafka.LinqToKafka.Tests;

internal class TestEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Test classes that mirror the structure used in MessagesQuerier.cs line 113
// which uses: messagesContext.GermanMessages.Where(x => x.Sender == filter.Sender)
internal class TestMessage
{
    public Guid Id { get; set; }
    public Guid Sender { get; set; }
    public string Content { get; set; } = string.Empty;
}

internal class TestFilter
{
    public Guid? Sender { get; set; }
}
