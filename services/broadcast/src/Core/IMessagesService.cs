namespace Core;

public interface IMessagesService
{
    Task Write(string message);
    Task<IEnumerable<string>> Lookup(MessagesFilter filter);
}