using System.Linq.Expressions;

namespace Kafka.LinqToKafka;

internal class Visitor(IQueryable newSource) : ExpressionVisitor
{
    protected override Expression VisitConstant(ConstantExpression node)
        => node.Value switch
        {
            IQueryable queryable when
                queryable.GetType().IsGenericType &&
                queryable.GetType().GetGenericTypeDefinition() == typeof(KafkaQueryable<>)
                => Expression.Constant(newSource),

            _ => base.VisitConstant(node)
        };
}