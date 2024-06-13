using AD.Api.Attributes.Services;
using System.Linq.Expressions;

namespace AD.Api.Expressions
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Transient)]
    public sealed class LambdaExpressionHasherVisitor : ExpressionVisitor
    {
        private int _hash;

        public LambdaExpressionHasherVisitor() { }

        public int GetHashCode(LambdaExpression? node)
        {
            if (node is null)
            {
                return 0;
            }

            _hash = 17;
            this.Visit(node);
            return _hash;
        }

        public void Reset()
        {
            _hash = 0;
        }

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node is null || node is not LambdaExpression lambdaNode)
            {
                return node;
            }

            unchecked
            {
                AddToHashCode(lambdaNode.NodeType, ref _hash);
                foreach (ParameterExpression p in lambdaNode.Parameters)
                {
                    AddToHashCode(p.Type, ref _hash);
                }

                AddToHashCode(lambdaNode.Body.Type, ref _hash);
            }

            return base.Visit(node);
        }

        private static void AddToHashCode<T>([DisallowNull] T obj, ref int hashCode)
        {
            hashCode = hashCode * 23 + obj.GetHashCode();
        }
    }
}

