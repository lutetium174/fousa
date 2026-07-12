namespace Core.Broker;

public interface IMessagesProducer
{
    Task Write(Message message, CancellationToken cancellationToken);
}