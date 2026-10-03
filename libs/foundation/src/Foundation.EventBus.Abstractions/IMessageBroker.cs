namespace Foundation.EventBus.Abstractions;

public interface IMessageBroker
{
    Task Publish<T>(string routingKey, T domainEvent, CancellationToken cancellationToken = default)
        where T : DomainEvent;
    
    Task Subscribe(string routingKey, Action<string, string> handler);
}

public record DomainEvent;