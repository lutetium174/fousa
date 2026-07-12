using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Kafka.LinqToKafka;

public class KafkaExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _builder = new();
    private HashSet<ParameterExpression> _lambdaParameters = [];
    private readonly Dictionary<ParameterExpression, object> _externalParameterValues = new();

    public string Query => _builder.ToString();
    
    // Handle +, -, ==, >, <, &&, ||, etc.
    protected override Expression VisitBinary(BinaryExpression node)
    {
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
            // Swap so column appears before value: "Column OP Value"
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

        // Prefer writing "Column OP Value" when one side is a column and the other is a value.
        // A column is a MemberExpression where the underlying expression is a lambda parameter.
        bool IsColumn(Expression e) => 
            e is MemberExpression { Expression: ParameterExpression param } && 
            _lambdaParameters.Contains(param);
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

    // Track lambda parameters to distinguish them from outer parameters
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        var oldLambdaParameters = _lambdaParameters;
        _lambdaParameters = new HashSet<ParameterExpression>(node.Parameters);
        
        var body = Visit(node.Body);
        
        _lambdaParameters = oldLambdaParameters;
        
        return node.Update(body, node.Parameters);
    }

    // Handle property/field access like u.Age or u.Name
    protected override Expression VisitMember(MemberExpression node)
    {
        switch (node.Expression)
        {
            case ConstantExpression constant:
                AddParameter(GetMemberValue(node, constant.Value));
                return node;
            case ParameterExpression param when _lambdaParameters.Contains(param):
                _builder.Append(node.Member.Name);
                return node;
            case ParameterExpression param when _externalParameterValues.TryGetValue(param, out var paramValue):
                AddParameter(GetMemberValue(node, paramValue));
                return node;
            case ParameterExpression _:
                _builder.Append(node.Member.Name);
                return node;
            default:
                _builder.Append(node.Member.Name);
                return node;
        }
    }

    // Handle literal values
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is IQueryable queryable)
        {
            var elementType = queryable.GetType().GetGenericArguments()[0];
            GetTableName(elementType, out var tableName);
            _builder.Append($"SELECT * FROM \"{tableName}\" ");
        }
        else if (node.Value?.GetType().IsGenericType == true && 
                 node.Value.GetType().GetGenericTypeDefinition() == typeof(KafkaQueryable<>))
        {
            var elementType = node.Value.GetType().GetGenericArguments()[0];
            GetTableName(elementType, out var tableName);
            _builder.Append($"SELECT * FROM \"{tableName}\" ");
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

    private StringBuilder AddParameter(object? value)
        => value switch
        {
            null => _builder.Append("NULL"),
            Guid guid => _builder.Append($"'{guid}'"),
            string str => _builder.Append($"'{str.Replace("'", "''")}'"),
            int or long or double or float or decimal or bool => _builder.Append(value),
            DateTime dateTime => _builder.Append($"'{dateTime:o}'"),
          
            _ => _builder.Append(value)
        };

    private static object? GetMemberValue(MemberExpression member, object? container)
        => member.Member switch
        {
            FieldInfo field => field.GetValue(container),
            PropertyInfo prop => prop.GetValue(container),

            _ => throw new NotSupportedException()
        };
}
