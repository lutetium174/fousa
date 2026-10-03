using System.Linq.Expressions;

namespace Core;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate) => predicate;
    public static Expression<Func<T, bool>> True<T>() => m => true;
    public static Expression<Func<T, bool>> False<T>() => m => false;

    public static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null) return right;
        if (right == null) return left;
        var parameter = Expression.Parameter(typeof(T), "x");
        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);
        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);
        var body = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null) return right;
        if (right == null) return left;
        var parameter = Expression.Parameter(typeof(T), "x");
        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);
        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);
        var body = Expression.OrElse(leftBody, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null) return True<T>();
        var parameter = Expression.Parameter(typeof(T), "x");
        var visitor = new ReplaceExpressionVisitor(predicate.Parameters[0], parameter);
        var body = Expression.Not(visitor.Visit(predicate.Body));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue, _newValue;
        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue) => (_oldValue, _newValue) = (oldValue, newValue);
        public override Expression? Visit(Expression? node) => node == _oldValue ? _newValue : base.Visit(node);
    }
}
