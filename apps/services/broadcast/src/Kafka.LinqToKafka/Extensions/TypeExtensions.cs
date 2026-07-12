namespace Kafka.LinqToKafka.Extensions;

public static class TypeExtensions
{
    public static Type GetSequenceElementType(this Type type)
    {
        var enumerableType = type.GetInterfaces()
            .Concat([type])
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        
        return enumerableType?.GetGenericArguments()[0] ?? type;
    }
}