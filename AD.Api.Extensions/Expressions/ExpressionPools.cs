using AD.Api.Expressions;
using AD.Api.Pooling;
using System.ComponentModel;

namespace AD.Api.Core.Pooling
{
    internal sealed class ExpressionComparerPool : ThreadedPoolBag<LambdaComparisonVisitor>, 
        IPoolBagLeaser<LambdaComparisonVisitor>
    {
        public ExpressionComparerPool(IServiceProvider provider)
            : base(provider)
        {
        }

        public LambdaComparisonVisitor Get()
        {
            return this.GetOrConstruct(out _);
        }
        public IPooledItem<LambdaComparisonVisitor> GetPooledItem()
        {
            return GetPooledItem(this, p => p.Get());
        }
        protected override bool Reset([DisallowNull] LambdaComparisonVisitor item)
        {
            item.Reset();
            return true;
        }

        //[DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddSingleton<ExpressionComparerPool>();
        }
    }

    internal sealed class ExpressionHasherPool : ThreadedPoolBag<LambdaExpressionHasherVisitor>,
        IPoolBagLeaser<LambdaExpressionHasherVisitor>
    {
        public ExpressionHasherPool(IServiceProvider provider) : base(provider)
        {
        }

        public LambdaExpressionHasherVisitor Get()
        {
            return this.GetOrConstruct(out _);
        }
        public IPooledItem<LambdaExpressionHasherVisitor> GetPooledItem()
        {
            return GetPooledItem(this, p => p.Get());
        }

        protected override bool Reset([DisallowNull] LambdaExpressionHasherVisitor item)
        {
            item.Reset();
            return true;
        }
    }
}
