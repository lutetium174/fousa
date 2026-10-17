using System.Collections;
using System.Linq.Expressions;

namespace Pinotchio;

public class OrderedPinotQueryable<T>(PinotQueryable<T> inner) : IOrderedQueryable<T>
{
    public Expression Expression => inner.Expression;
    public Type ElementType => inner.ElementType;
    public IQueryProvider Provider => inner.Provider;

    public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}