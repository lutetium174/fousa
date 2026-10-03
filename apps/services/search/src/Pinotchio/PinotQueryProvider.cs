using System.Linq.Expressions;
using Messages.Search;
using Messages.Search.Mappers;

namespace Pinotchio;

internal class QueryProvider(IPinotClient client) : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = expression.Type.GetGenericArguments().First();
        var queryableType = typeof(PinotQueryable<>).MakeGenericType(elementType);
        return (IQueryable)Activator.CreateInstance(queryableType, this, expression)!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new PinotQueryable<TElement>(this, expression);

    public object Execute(Expression expression)
        => Execute<object>(expression);

    public TResult Execute<TResult>(Expression expression)
    {
        var sql = ExpressionTranslator.Translate(expression);
        var json = client.ExecuteSql(sql);

        return IsEnumerableButNotString(typeof(TResult), out var elementType)
            ? (TResult)typeof(ResultMapper)
                .GetMethod(nameof(ResultMapper.Map))!
                .MakeGenericMethod(elementType)
                .Invoke(null, [json])!
            : ScalarMapper.Map<TResult>(json);
    }

    private static bool IsEnumerableButNotString(Type type, out Type elementType)
    {
        elementType = null!;

        if (type == typeof(string))
            return false;

        if (type.IsGenericType &&
            typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        var enumerable = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerable == null) return false;
        elementType = enumerable.GetGenericArguments()[0];
        return true;
    }
}