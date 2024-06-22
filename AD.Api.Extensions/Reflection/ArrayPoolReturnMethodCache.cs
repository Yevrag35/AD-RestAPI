using AD.Api.Attributes.Services;
using AD.Api.Pooling;
using System.Buffers;
using System.Collections.Concurrent;

namespace AD.Api.Reflection
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Singleton)]
    public sealed class ArrayPoolReturnMethodCache
    {
        private static readonly string _methodName = nameof(ArrayPool<object>.Return);
        private readonly GenericMethodDictionary<Type> _cache;
        private readonly ConcurrentDictionary<Type, object> _pools;
        private readonly NonLeaseablePool<object[]> _parameterPool;

        public int Count => _cache.Count;
        public Type TypeDefinition { get; }

        public ArrayPoolReturnMethodCache()
        {
            _cache = new(10);
            _pools = new(Environment.ProcessorCount, 10);
            _parameterPool = new(10, PrefillBag(5), () => new object[2], null);
            this.TypeDefinition = typeof(ArrayPool<>);
        }

        private static IEnumerable<object[]> PrefillBag(int count)
        {
            return Enumerable.Repeat(new object[2], count);
        }
        public void Return(Array array, Type? elementType, bool clearArray)
        {
            if (elementType is null)
            {
                elementType = array.GetType().GetElementType();
                ArgumentNullException.ThrowIfNull(elementType);
            }

            Type? poolType = null;
            if (!_pools.TryGetValue(elementType, out object? pool))
            {
                pool = this.GetArrayPool(elementType, out poolType);
            }

            if (!_cache.TryGetValue(elementType, out MethodInfo? method))
            {
                method = this.GetReturnMethod(poolType, elementType);
            }

            ReturnArray(array, pool, clearArray, _parameterPool, method);
        }

        private object GetArrayPool(Type elementType, out Type poolType)
        {
            poolType = this.TypeDefinition.MakeGenericType(elementType);
            PropertyInfo property = poolType.GetProperty(nameof(ArrayPool<object>.Shared),
                BindingFlags.Public | BindingFlags.Static)
                    ?? throw new MissingMemberException(poolType.GetName(), nameof(ArrayPool<object>.Shared));

            object pool = property.GetValue(null) ?? throw new InvalidOperationException("Somehow this is null.");
            _pools.TryAdd(elementType, pool);
            return pool;
        }
        private MethodInfo GetReturnMethod(Type? genericType, Type elementType)
        {
            genericType ??= typeof(ArrayPool<>).MakeGenericType(elementType);

            MethodInfo methodInfo = genericType.GetMethod(_methodName, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new MissingMemberException(genericType.GetName(), _methodName);

            _cache.TryAdd(elementType, methodInfo);
            return methodInfo;
        }

        private static void ReturnArray(Array array, object pool, bool clearArray, NonLeaseablePool<object[]> bag, MethodInfo returnMethod)
        {
            object[] parameters = bag.Get();
            
            try
            {
                parameters[0] = array;
                parameters[1] = clearArray;
                returnMethod.Invoke(pool, parameters);
            }
            finally
            {
                bag.Return(parameters);
            }
        }
    }
}

