using AD.Api.Attributes.Services;
using System.Linq.Expressions;

namespace AD.Api.Expressions
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Transient)]
    public sealed class LambdaComparisonVisitor : ExpressionVisitor
    {
        private LambdaExpression? _other;
        private bool _result;
        private readonly Stack<LambdaExpression> _stack = new();

        public LambdaComparisonVisitor() { }

        public bool AreEqual(LambdaExpression left, LambdaExpression right)
        {
            _other = right;
            _result = true;
            this.Visit(left);
            return _result;
        }

        [return: NotNullIfNotNull("node")]
        public override Expression? Visit(Expression? node)
        {
            if (node is null || _other is null)
            {
                _result = _result && (node == _other);
                return node;
            }

            if (node is not LambdaExpression lambdaNode)
            {
                return node;
            }

            if (lambdaNode.Body.Type.Equals(_other.Body.Type))
            {
                _stack.Push(_other);
                _other = lambdaNode;
                base.Visit(lambdaNode);
                _other = _stack.Pop();
            }
            else
            {
                _result = false;
            }

            return lambdaNode;
        }

        public void Reset()
        {
            _result = false;
            _other = null;
            _stack.Clear();
        }
    }
}
