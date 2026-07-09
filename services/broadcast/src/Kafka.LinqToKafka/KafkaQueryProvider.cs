using System.Linq.Expressions;
using System.Reflection;
using Kafka.LinqToKafka.Extensions;

namespace Kafka.LinqToKafka;

public class KafkaQueryProvider(IQueryable source) : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
        => (IQueryable)typeof(KafkaQueryable<>)
            .MakeGenericType(expression.Type.GetSequenceElementType())
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [typeof(KafkaQueryProvider), typeof(Expression)],
                null)
            !.Invoke([this, expression]);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new KafkaQueryable<TElement>(this, expression);

    public object Execute(Expression expression)
        => Execute<object>(expression);

    public TResult Execute<TResult>(Expression expression)
        => source.Provider
            .Execute<TResult>(new Visitor(source).Visit(expression));
}