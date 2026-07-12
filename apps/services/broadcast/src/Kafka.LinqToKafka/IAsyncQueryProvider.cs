using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.LinqToKafka;

/// <summary>
/// Asynchronous version of IQueryProvider for executing queries asynchronously.
/// Similar to Entity Framework's IAsyncQueryProvider.
/// </summary>
public interface IAsyncQueryProvider : IQueryProvider
{
    /// <summary>
    /// Executes the query represented by the specified expression tree asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="expression">The expression representing the query.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation, containing the query result.</returns>
    Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
}
