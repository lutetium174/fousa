using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Kafka.LinqToKafka;

public class KafkaExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _builder = new();
    private readonly Dictionary<string, object> _parameters = new();
    private int _parameterIndex;

    public string Query => _builder.ToString();
    public IReadOnlyDictionary<string, object> Parameters => _parameters;

    // Handle +, -, ==, >, <, &&, ||, etc.
    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Prefer writing "Column OP Value" when one side is a member (parameter) and the other is a constant/parameter.
        bool IsColumn(Expression e) => e is MemberExpression me && me.Expression is ParameterExpression;

        var opText = node.NodeType switch
        {
            ExpressionType.Equal => " = ",
            ExpressionType.NotEqual => " <> ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.OrElse => " OR ",

            _ => throw new NotSupportedException($"Binary operator {node.NodeType} is not supported")
        };

        _builder.Append('(');

        var leftIsCol = IsColumn(node.Left);
        var rightIsCol = IsColumn(node.Right);

        if (!leftIsCol && rightIsCol)
        {
            // Swap so column appears before parameter: "Column OP @p0"
            Visit(node.Right);
            _builder.Append(opText);
            Visit(node.Left);
        }
        else
        {
            Visit(node.Left);
            _builder.Append(opText);
            Visit(node.Right);
        }

        _builder.Append(')');

        return node;
    }

    // Handle method calls like .Where(), .Select(), .Contains()
    protected override Expression VisitMethodCall(MethodCallExpression node)
        => node.Method.Name switch
        {
            "Where" => Pipe(node, () => _builder.Append(" WHERE ")),
            "OrderBy" => PipeParameterised(
                node,
                () => _builder.Append(" ORDER BY "),
                () => _builder.Append(" ASC ")),
            "OrderByDescending" => PipeParameterised(
                node,
                () => _builder.Append(" ORDER BY "),
                () => _builder.Append(" DESC ")),
            "Take" => Pipe(node, () => _builder.Append(" LIMIT ")),
            "Skip" => Pipe(node, () => _builder.Append(" OFFSET ")),

            _ => throw new NotSupportedException($"Method {node.Method.Name} is not supported")
        };

    private MethodCallExpression PipeParameterised(MethodCallExpression node, Action pre, Action post)
    {
        Visit(node.Arguments[0]);
        pre();
        Visit(node.Arguments[1]);
        post();

        return node;
    }

    private MethodCallExpression Pipe(MethodCallExpression node, Action build)
    {
        Visit(node.Arguments[0]);
        build();
        Visit(node.Arguments[1]);

        return node;
    }

    // Handle property/field access like u.Age or u.Name
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ConstantExpression constant)
        {
            var value = GetMemberValue(node, constant.Value);
            AddParameter(value);
        }
        else
            _builder.Append(node.Member.Name);

        return node;
    }

    // Handle literal values
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is IQueryable queryable)
        {
            var elementType = queryable.GetType().GetGenericArguments()[0];
            GetTableName(elementType, out var tableName);
            _builder.Append("FROM \"");
            _builder.Append(tableName);
            _builder.Append("\" ");
        }
        else if (node.Value?.GetType().IsGenericType == true && 
                 node.Value.GetType().GetGenericTypeDefinition() == typeof(KafkaQueryable<>))
        {
            // Handle KafkaQueryable constant - extract the element type
            var elementType = node.Value.GetType().GetGenericArguments()[0];
            GetTableName(elementType, out var tableName);
            _builder.Append("FROM \"");
            _builder.Append(tableName);
            _builder.Append("\" ");
        }
        else
            AddParameter(node.Value);

        return node;
    }

    private void GetTableName(Type type, out string tableName)
    {
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Name))
        {
            tableName = tableAttr.Name;
            return;
        }
        
        tableName = type.Name;
    }

    private void AddParameter(object? value)
    {
        var paramName = $"@p{_parameterIndex++}";
        _parameters[paramName] = value ?? DBNull.Value;
        _builder.Append(paramName);
    }

    private static object? GetMemberValue(MemberExpression member, object? container)
        => member.Member switch
        {
            FieldInfo field => field.GetValue(container),
            PropertyInfo prop => prop.GetValue(container),

            _ => throw new NotSupportedException()
        };
}
