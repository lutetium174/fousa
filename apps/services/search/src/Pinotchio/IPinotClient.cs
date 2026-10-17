namespace Pinotchio;

public interface IPinotClient
{
    Task<string> ExecuteSql(string sql, CancellationToken cancellationToken);
}
