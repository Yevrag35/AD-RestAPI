using System.Linq.Expressions;

namespace AD.Api.Expressions
{
    public class ExpressionComparisonVisitor : ExpressionVisitor
    {
        private Expression? _other;
        private bool _result;
        private readonly Stack<Expression> _stack = new();

        public bool AreEqual(Expression left, Expression right)
        {
            _other = right;
            _stack.Push(left);
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

            if (_result && node.NodeType == _other.NodeType && node.Type.Equals(_other.Type))
            {
                _stack.Push(_other);
                _other = node;
                base.Visit(node);
                _other = _stack.Pop();
            }
            else
            {
                _result = false;
            }

            return node;
        }

        public void Reset()
        {
            _result = false;
            _other = null;
            _stack.Clear();
        }
    }
}
