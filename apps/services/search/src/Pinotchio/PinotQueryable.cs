using System.Collections;
using System.Linq.Expressions;

namespace Pinotchio;

public class PinotQueryable<T> : IQueryable<T>
{
    public Expression Expression { get; }
    public Type ElementType => typeof(T);
    public IQueryProvider Provider { get; }

    public PinotQueryable(IQueryProvider provider)
    {
        Provider = provider;
        Expression = Expression.Constant(this);
    }

    public PinotQueryable(IQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
    }

    public IEnumerator<T> GetEnumerator()
        => Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}