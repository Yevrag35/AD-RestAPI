using AD.Api.Attributes.Services;
using AD.Api.Core.Pooling;
using AD.Api.Pooling;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AD.Api.Expressions
{
    [DynamicDependencyRegistration]
    public sealed class LambdaExpressionEqualityComparer : IEqualityComparer<LambdaExpression>
    {
        private readonly IPoolBagLeaser<LambdaComparisonVisitor> _comparePool;
        private readonly IPoolBagLeaser<LambdaExpressionHasherVisitor> _hashPool;

        public LambdaExpressionEqualityComparer(IPoolBagLeaser<LambdaComparisonVisitor> comparerPool, IPoolBagLeaser<LambdaExpressionHasherVisitor> hasherPool)
        {
            _comparePool = comparerPool;
            _hashPool = hasherPool;
        }

        public bool Equals(LambdaExpression? x, LambdaExpression? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            using (var comparer = _comparePool.GetPooledItem())
            {
                return comparer.Value.AreEqual(x, y);
            }
        }

        public int GetHashCode([DisallowNull] LambdaExpression obj)
        {
            using (var hasher = _hashPool.GetPooledItem())
            {
                return hasher.Value.GetHashCode(obj);
            }
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddSingleton<LambdaExpressionEqualityComparer>();
            AddPoolToService<LambdaComparisonVisitor, ExpressionComparerPool>(services);
            AddPoolToService<LambdaExpressionHasherVisitor, ExpressionHasherPool>(services);
        }
        private static void AddPoolToService<TItem, TImplementation>(IServiceCollection services)
            where TItem : class
            where TImplementation : class, IPoolBagLeaser<TItem>
        {
            static TImplementation factory(IServiceProvider x) => x.GetRequiredService<TImplementation>();

            services.AddSingleton<TImplementation>()
                        .AddSingleton<IPoolBagLeaser<TItem>>(factory)
                        .AddSingleton<IPoolBag<TItem>>(factory)
                        .AddSingleton<IPoolReturner<TItem>>(factory)
                        .AddSingleton<IPoolLeaseReturner<TItem>>(factory)
                        .AddScoped(x =>
                        {
                            TImplementation imp = factory(x);
                            return imp.GetPooledItem();
                        });
        }
    }
}
