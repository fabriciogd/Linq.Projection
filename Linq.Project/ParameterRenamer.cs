namespace Linq.Projection
{
    using System.Linq.Expressions;

    public class ParameterRenamer : ExpressionVisitor
    {
        private ParameterExpression _parameterExpression;
        private string _parameterExpressionName;

        public Expression Rename(Expression expression, ParameterExpression parameterExpression)
        {
            this._parameterExpression = parameterExpression;
            return Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (this._parameterExpressionName == null)
                this._parameterExpressionName = node.Name;

            return node.Name == this._parameterExpressionName && node.Type == this._parameterExpression.Type ? this._parameterExpression : node;
        }
    }
}
