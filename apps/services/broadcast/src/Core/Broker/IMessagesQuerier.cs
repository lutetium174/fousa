using Core.Responses;
using Foundation;

namespace Core.Broker;

public interface IMessagesQuerier
{
    Task<Result<IEnumerable<MessageResponse>>> Lookup(MessagesFilter filter, CancellationToken  cancellationToken);
}