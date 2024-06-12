using AD.Api.Expressions;

namespace AD.Api.Core.Pooling
{
    internal sealed class ExpressionComparerPool : ThreadedPoolBag<ExpressionComparisonVisitor>
    {
        protected override bool Reset([DisallowNull] ExpressionComparisonVisitor item)
        {
            item.Reset();
            return true;
        }
    }
}
