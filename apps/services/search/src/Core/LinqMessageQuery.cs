using System.Linq;
using System.Linq.Expressions;

namespace Core;

public class LinqMessageQuery : IMessageQuery
{
    private List<Expression<Func<Message, bool>>> _predicates = new();
    private List<Expression<Func<Message, bool>>> _andPredicates = new();
    private List<Expression<Func<Message, bool>>> _orPredicates = new();
    private bool _isNegated = false;

    public Expression<Func<Message, bool>>? Predicate { get; private set; }
    public List<(Expression<Func<Message, object>> KeySelector, SortDirection Direction)> SortExpressions { get; private set; } = new();
    public Expression<Func<Message, object>>? Selector { get; private set; }
    public int? Limit { get; private set; }
    public int? Offset { get; private set; }
    public int? Page { get; private set; }
    public int? PageSize { get; private set; }
    public string[] Includes { get; private set; } = Array.Empty<string>();

    public void AddPredicate(Expression<Func<Message, bool>> predicate, bool isAnd = true)
    {
        if (isAnd) _andPredicates.Add(predicate);
        else _orPredicates.Add(predicate);
    }

    public void SetNegated(bool isNegated = true) => _isNegated = isNegated;
    public void SetSorting(List<(Expression<Func<Message, object>> KeySelector, SortDirection Direction)> sortExpressions)
    {
        SortExpressions.Clear();
        SortExpressions.AddRange(sortExpressions);
    }
    public void ThenSort(Expression<Func<Message, object>> keySelector, SortDirection direction) => SortExpressions.Add((keySelector, direction));
    public void SetSelector<TResult>(Expression<Func<Message, TResult>> selector) => Selector = selector as Expression<Func<Message, object>>;
    public void SetPagination(int? limit, int? offset, int? page, int? pageSize)
    {
        Limit = limit; Offset = offset; Page = page; PageSize = pageSize;
    }
    public void SetIncludes(string[] includes) => Includes = includes;

    public void CompilePredicate()
    {
        if (_predicates.Count == 0 && _andPredicates.Count == 0 && _orPredicates.Count == 0) { Predicate = null; return; }

        var combined = CombinePredicates(_andPredicates, Expression.AndAlso);
        if (_orPredicates.Count > 0)
        {
            var orCombined = CombinePredicates(_orPredicates, Expression.OrElse);
            combined = combined != null ? Expression.Lambda<Func<Message, bool>>(Expression.OrElse(combined.Body, orCombined.Body), combined.Parameters[0]) : orCombined;
        }
        if (_predicates.Count > 0)
        {
            var mainCombined = CombinePredicates(_predicates, Expression.AndAlso);
            combined = combined != null ? Expression.Lambda<Func<Message, bool>>(Expression.AndAlso(combined.Body, mainCombined.Body), combined.Parameters[0]) : mainCombined;
        }
        if (_isNegated && combined != null) combined = Expression.Lambda<Func<Message, bool>>(Expression.Not(combined.Body), combined.Parameters[0]);
        Predicate = combined;
    }

    private Expression<Func<Message, bool>>? CombinePredicates(List<Expression<Func<Message, bool>>> predicates, Func<Expression, Expression, BinaryExpression> combiner)
    {
        if (predicates.Count == 0) return null;
        if (predicates.Count == 1) return predicates[0];
        var parameter = Expression.Parameter(typeof(Message), "m");
        Expression? body = null;
        foreach (var predicate in predicates)
        {
            var visitor = new ReplaceExpressionVisitor(predicate.Parameters[0], parameter);
            var newBody = visitor.Visit(predicate.Body);
            body = body == null ? newBody : combiner(body, newBody);
        }
        return body != null ? Expression.Lambda<Func<Message, bool>>(body, parameter) : null;
    }

    public IMessageQuery And(IMessageQuery other)
    {
        if (other is LinqMessageQuery o) {
            var r = new LinqMessageQuery { _andPredicates = new(_andPredicates), _orPredicates = new(_orPredicates), _predicates = new(_predicates), _isNegated = _isNegated, SortExpressions = new(SortExpressions), Selector = Selector, Limit = Limit, Offset = Offset, Page = Page, PageSize = PageSize, Includes = Includes };
            r._andPredicates.AddRange(o._andPredicates); r._orPredicates.AddRange(o._orPredicates); r._predicates.AddRange(o._predicates);
            return r;
        }
        throw new NotSupportedException("AND operation is only supported between LinqMessageQuery instances");
    }

    public IMessageQuery Or(IMessageQuery other)
    {
        if (other is LinqMessageQuery o) {
            var r = new LinqMessageQuery { _isNegated = _isNegated || o._isNegated, SortExpressions = new(SortExpressions), Selector = Selector ?? o.Selector, Limit = Limit ?? o.Limit, Offset = Offset ?? o.Offset, Page = Page ?? o.Page, PageSize = PageSize ?? o.PageSize, Includes = Includes.Length > 0 ? Includes : o.Includes };
            r._orPredicates.AddRange(_andPredicates); r._orPredicates.AddRange(_orPredicates); r._orPredicates.AddRange(_predicates);
            r._orPredicates.AddRange(o._andPredicates); r._orPredicates.AddRange(o._orPredicates); r._orPredicates.AddRange(o._predicates);
            return r;
        }
        throw new NotSupportedException("OR operation is only supported between LinqMessageQuery instances");
    }

    public IMessageQuery Not() => new LinqMessageQuery { _andPredicates = new(_andPredicates), _orPredicates = new(_orPredicates), _predicates = new(_predicates), _isNegated = !_isNegated, SortExpressions = new(SortExpressions), Selector = Selector, Limit = Limit, Offset = Offset, Page = Page, PageSize = PageSize, Includes = Includes };

    public IQueryable<Message> ApplyTo(IQueryable<Message> source)
    {
        var query = source;
        if (Predicate != null) query = query.Where(Predicate);
        
        if (SortExpressions.Count > 0)
        {
            var firstSort = SortExpressions[0];
            var orderedQuery = firstSort.Direction == SortDirection.Ascending ? query.OrderBy(firstSort.KeySelector) : query.OrderByDescending(firstSort.KeySelector);
            for (int i = 1; i < SortExpressions.Count; i++) {
                var sort = SortExpressions[i];
                orderedQuery = sort.Direction == SortDirection.Ascending ? orderedQuery.ThenBy(sort.KeySelector) : orderedQuery.ThenByDescending(sort.KeySelector);
            }
            query = orderedQuery;
        }
        else query = query.OrderByDescending(m => m.CreatedAt);
        
        if (Page.HasValue && PageSize.HasValue) query = query.Skip((Page.Value - 1) * PageSize.Value).Take(PageSize.Value);
        else { if (Offset.HasValue) query = query.Skip(Offset.Value); if (Limit.HasValue) query = query.Take(Limit.Value); }
        return query;
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue, _newValue;
        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue) => (_oldValue, _newValue) = (oldValue, newValue);
        public override Expression? Visit(Expression? node) => node == _oldValue ? _newValue : base.Visit(node);
    }
}
