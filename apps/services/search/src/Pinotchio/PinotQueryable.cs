using System.Collections;
using System.Linq.Expressions;

namespace Pinotchio;

public class Queryable<T> : IQueryable<T>
{
    public Expression Expression { get; }
    public Type ElementType => typeof(T);
    public IQueryProvider Provider { get; }

    public Queryable(IQueryProvider provider)
    {
        Provider = provider;
        Expression = Expression.Constant(this);
    }

    public Queryable(IQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
    }

    public IEnumerator<T> GetEnumerator()
        => Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}