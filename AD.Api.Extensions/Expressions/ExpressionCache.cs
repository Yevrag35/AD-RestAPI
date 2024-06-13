using AD.Api.Attributes.Services;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace AD.Api.Expressions
{
    public interface IExpressionCache<TInput, TOutput>
    {
        Func<TInput, TOutput> GetOrAdd(Expression<Func<TInput, TOutput>> expression);
    }

    [DependencyRegistration(typeof(IExpressionCache<,>), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class ExpressionCache<TInput, TOutput> : IExpressionCache<TInput, TOutput>
    {
        private readonly ConcurrentDictionary<Expression<Func<TInput, TOutput>>, Func<TInput, TOutput>> _cache;

        public ExpressionCache(LambdaExpressionEqualityComparer comparer)
        {
            _cache = new(Environment.ProcessorCount, 0, comparer);
        }

        public Func<TInput, TOutput> GetOrAdd(Expression<Func<TInput, TOutput>> expression)
        {
            return _cache.GetOrAdd(expression, e => e.Compile());
        }
    }
}

