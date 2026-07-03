using Foundation;

namespace Core.Broker;

public interface IMessagesQuerier
{
    Task<Result<IEnumerable<string>>> Lookup(MessagesFilter filter, CancellationToken  cancellationToken);
}