using System.Linq.Expressions;
using System.Text;

namespace Pinotchio;

internal class ExpressionTranslator(string tableName, bool supportOrderBy = false) :
    ExpressionVisitor
{
    private readonly StringBuilder _stringBuilder = new();
    private bool _hasWhereClause;

    public static string Translate(Expression expression, string tableName, bool supportOrderBy = true)
    {
        var expressionTranslator = new ExpressionTranslator(tableName, supportOrderBy);
        expressionTranslator.Visit(expression);
        return expressionTranslator._stringBuilder.ToString();
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (!node.Type.GetInterfaces()
                .Any((Func<Type, bool>)(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>))) ||
            _stringBuilder.Length != 0)
            return base.VisitConstant(node);
        var interpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, _stringBuilder);
        interpolatedStringHandler.AppendLiteral("SELECT * FROM ");
        interpolatedStringHandler.AppendFormatted(tableName);
        ref var local = ref interpolatedStringHandler;
        _stringBuilder.Append(ref local);
        return base.VisitConstant(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(string))
            return HandleStringMethodCall(node);

        if (node.Method.DeclaringType != typeof(Queryable))
            return base.VisitMethodCall(node);

        return node.Method.Name switch
        {
            "Any" => VisitAny(node),
            "Count" => VisitCount(node),
            "First" or "FirstOrDefault" => VisitFirst(node),
            "Last" or "LastOrDefault" => VisitLast(node),
            "LongCount" => VisitCount(node),
            "OrderBy" => VisitOrderBy(node, false),
            "OrderByDescending" => VisitOrderBy(node, true),
            "Select" or "Skip" or "Take" => VisitExpression(node.Arguments[0]),
            "Single" or "SingleOrDefault" => VisitSingle(node),
            "ThenBy" => VisitThenBy(node, false),
            "ThenByDescending" => VisitThenBy(node, true),
            "Where" => VisitWhere(node),
            
            _ => base.VisitMethodCall(node)
        };
    }

    private Expression HandleStringMethodCall(MethodCallExpression node)
    {
        switch (node.Method.Name)
        {
            case "Contains":
                if (node.Arguments.Count > 0)
                {
                    _stringBuilder.Append("TEXT_MATCH(");
                    Visit(node.Object);
                    _stringBuilder.Append(", '");
                    HandleStringArgument(node.Arguments[0]);
                    _stringBuilder.Append("')");
                    break;
                }

                goto default;
            case "Equals":
                if (node.Arguments.Count > 0)
                {
                    Visit(node.Object);
                    _stringBuilder.Append(" = ");
                    if (node.Arguments[0] is ConstantExpression { Value: not null } constantExpression)
                    {
                        _stringBuilder.Append($"'{$"{constantExpression.Value}".Replace("'", "''")}'");
                        break;
                    }

                    Visit(node.Arguments[0]);
                    break;
                }

                goto default;
            case "StartsWith":
                if (node.Arguments.Count > 0)
                {
                    Visit(node.Object);
                    _stringBuilder.Append(" LIKE '");
                    HandleStringArgument(node.Arguments[0]);
                    _stringBuilder.Append("%'");
                    break;
                }

                goto default;
            case "EndsWith":
                if (node.Arguments.Count > 0)
                {
                    Visit(node.Object);
                    _stringBuilder.Append(" LIKE '%");
                    HandleStringArgument(node.Arguments[0]);
                    _stringBuilder.Append('\'');
                    break;
                }

                goto default;
            default:
                return base.VisitMethodCall(node);
        }

        return node;
    }

    private void HandleStringArgument(Expression argument)
    {
        if (argument is ConstantExpression { Value: not null } constantExpression)
            _stringBuilder.Append($"{constantExpression.Value}".Replace("'", "''"));
        else
            Visit(argument);
    }

    private Expression VisitWhere(MethodCallExpression node)
    {
        VisitExpression(node.Arguments[0]);
        if (!_hasWhereClause)
        {
            _stringBuilder.Append(" WHERE ");
            _hasWhereClause = true;
        }
        else
            _stringBuilder.Append(" AND ");

        return VisitExpression(node.Arguments[1]);
    }

    private Expression VisitOrderBy(MethodCallExpression node, bool descending)
    {
        if (!supportOrderBy)
        {
            VisitExpression(node.Arguments[0]);
            return node;
        }

        VisitExpression(node.Arguments[0]);
        _stringBuilder.Append(" ORDER BY ");
        Visit(node.Arguments[1]);
        if (descending)
            _stringBuilder.Append(" DESC");
        return node;
    }

    private Expression VisitThenBy(MethodCallExpression node, bool descending)
    {
        if (!supportOrderBy)
        {
            VisitExpression(node.Arguments[0]);
            return node;
        }

        _stringBuilder.Append(", ");
        Visit(node.Arguments[1]);
        if (descending)
            _stringBuilder.Append(" DESC");
        return node;
    }

    private Expression VisitCount(MethodCallExpression node)
    {
        VisitExpression(node.Arguments[0]);
        string str = _stringBuilder.ToString();
        if (str.StartsWith("SELECT * FROM"))
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("SELECT COUNT(*) FROM" + str.Substring("SELECT * FROM".Length));
        }

        return node;
    }

    private Expression VisitAny(MethodCallExpression node)
    {
        VisitExpression(node.Arguments[0]);
        string str = _stringBuilder.ToString();
        if (str.StartsWith("SELECT * FROM"))
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("SELECT COUNT(*) > 0 FROM" + str.Substring("SELECT * FROM".Length));
        }

        return node;
    }

    private MethodCallExpression VisitFirst(MethodCallExpression node)
    {
        VisitExpression(node.Arguments[0]);
        if (!_stringBuilder.ToString().Contains("LIMIT"))
            _stringBuilder.Append(" LIMIT 1");
        return node;
    }

    private MethodCallExpression VisitSingle(MethodCallExpression node)
    {
        VisitExpression(node.Arguments[0]);
        if (!_stringBuilder.ToString().Contains("LIMIT"))
            _stringBuilder.Append(" LIMIT 2");
        return node;
    }

    private MethodCallExpression VisitLast(MethodCallExpression node)
    {
        VisitExpression(node.Arguments[0]);
        if (!_stringBuilder.ToString().Contains("LIMIT"))
            _stringBuilder.Append(" LIMIT 1");
        return node;
    }

    private Expression VisitExpression(Expression node)
    {
        Visit(node);
        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _stringBuilder.Append('(');
        Visit(node.Left);

        var str = node.NodeType switch
        {
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Equal => " = ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.NotEqual => " != ",
            ExpressionType.OrElse => " OR ",
            
            _ => $" {node.NodeType.ToString()} "
        };

        _stringBuilder.Append(str);
        Visit(node.Right);
        _stringBuilder.Append(')');
        
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member.Name == "term")
            return node;
        var name = node.Member.Name;
        if (node.Member.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() is ColumnAttribute
            columnAttribute)
            name = columnAttribute.Name;
        var str = _stringBuilder.ToString();
        if (string.IsNullOrEmpty(str) ||
            str.EndsWith('(') ||
            str.EndsWith(' ') ||
            str.EndsWith("WHERE ") ||
            str.EndsWith("AND ") ||
            str.EndsWith("OR "))
            _stringBuilder.Append(name);
        else
            _stringBuilder.Append("." + name);
        
        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType != ExpressionType.Not)
            return base.VisitUnary(node);
        _stringBuilder.Append("NOT ");
        Visit(node.Operand);
        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node) => node;

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        Visit(node.Body);
        return node;
    }
}