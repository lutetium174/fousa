using System.Linq.Expressions;
using System.Text;

namespace Pinotchio;

internal class PinotExpressionTranslator : ExpressionVisitor
{
    private readonly StringBuilder _stringBuilder = new();

    public static string Translate(Expression expression)
    {
        var visitor = new PinotExpressionTranslator();
        visitor.Visit(expression);
        return visitor._stringBuilder.ToString();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Handle Where, Select, OrderBy, Take, etc.
        return base.VisitMethodCall(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Handle ==, >, <, &&, ||
        return base.VisitBinary(node);
    }
}
