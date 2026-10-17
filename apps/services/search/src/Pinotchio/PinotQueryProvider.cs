using System.Linq.Expressions;
using Pinotchio.Mappers;

namespace Pinotchio;

internal sealed class PinotQueryProvider(
    IPinotClient client,
    string tableName,
    bool supportOrderBy = false) : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
        => (IQueryable)Activator.CreateInstance(typeof(PinotQueryable<>)
            .MakeGenericType(expression.Type.GetGenericArguments().First()), this, expression)!;

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var queryable = new PinotQueryable<TElement>(this, expression);

        return expression is MethodCallExpression
        {
            Method.Name:
            "OrderBy" or
            "OrderByDescending" or
            "ThenBy" or
            "ThenByDescending"
        }
            ? new OrderedPinotQueryable<TElement>(queryable)
            : queryable;
    }

    public object Execute(Expression expression) => Execute<object>(expression);

    public TResult Execute<TResult>(Expression expression)
    {
        var result = client
            .ExecuteSql(ExpressionTranslator.Translate(expression, tableName, supportOrderBy), CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        if (!IsEnumerableButNotString(typeof(TResult), out var elementType))
            return ScalarMapper.Map<TResult>(result);

        return (TResult)typeof(ResultMapper)
            .GetMethod("Map")!
            .MakeGenericMethod(elementType)
            .Invoke(null, [result])!;
    }

    public Task<object> ExecuteAsync(Expression expression,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<object>(expression, cancellationToken);
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Expression expression,
        CancellationToken cancellationToken = default)
    {
        var sql = ExpressionTranslator.Translate(expression, tableName, supportOrderBy);
        var json = await client.ExecuteSql(sql, cancellationToken).ConfigureAwait(false);

        if (!IsEnumerableButNotString(typeof(TResult), out var elementType))
            return ScalarMapper.Map<TResult>(json);

        return (TResult)typeof(ResultMapper)
            .GetMethod("Map")!
            .MakeGenericMethod(elementType).Invoke(null, [json])!;
    }

    private static bool IsEnumerableButNotString(Type type, out Type? elementType)
    {
        elementType = null;

        if (type == typeof(string))
            return false;

        if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        var type1 = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (type1 == null)
            return false;

        elementType = type1.GetGenericArguments()[0];
        return true;
    }
}