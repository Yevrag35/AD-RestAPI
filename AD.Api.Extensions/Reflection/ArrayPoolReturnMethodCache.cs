using AD.Api.Attributes.Services;
using AD.Api.Pooling;
using System.Buffers;
using System.Collections.Concurrent;

namespace AD.Api.Reflection
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Singleton)]
    public sealed class ArrayPoolReturnMethodCache
    {
        private static readonly string _returnName = nameof(ArrayPool<byte>.Return);
        private static readonly string _sharedName = nameof(ArrayPool<byte>.Shared);
        private readonly GenericMethodDictionary<Type> _cache;
        private readonly ConcurrentDictionary<Type, object> _pools;
        private readonly NonLeaseablePool<object[]> _parameterPool;
        private readonly NonLeaseablePool<Type[]> _typePool;

        public int Count => _cache.Count;
        public Type TypeDefinition { get; }

        public ArrayPoolReturnMethodCache()
        {
            _cache = new(10);
            _pools = new(Environment.ProcessorCount, 10);
            static object[] factory() => new object[2];
            static Type[] typeArrayFactory() => new Type[1];

            _parameterPool = new(10, PrefillBag(5, factory), factory, null);
            _typePool = new(10, PrefillBag(2, typeArrayFactory), typeArrayFactory, null);
            this.TypeDefinition = typeof(ArrayPool<>);
        }
        
        public void Return(Array array, Type? elementType, bool clearArray)
        {
            elementType ??= GetElementType(array);

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
            PropertyInfo property = poolType.GetProperty(_sharedName,
                BindingFlags.Public | BindingFlags.Static)
                    ?? throw new MissingMemberException(poolType.GetName(), _sharedName);

            object pool = property.GetValue(null) ?? throw new InvalidOperationException("Somehow this is null.");
            _pools.TryAdd(elementType, pool);
            return pool;
        }
        public static Type GetElementType(Array array)
        {
            return array.GetType().GetElementType() ?? typeof(object);
        }
        private Type GetGenericType(Type elementType)
        {
            Type[] parameters = _typePool.Get();

            try
            {
                parameters[0] = elementType;
                return this.TypeDefinition.MakeGenericType(parameters);
            }
            finally
            {
                _typePool.Return(parameters);
            }
        }
        private MethodInfo GetReturnMethod(Type? genericType, Type elementType)
        {
            genericType ??= this.GetGenericType(elementType);

            MethodInfo methodInfo = genericType.GetMethod(_returnName, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new MissingMemberException(genericType.GetName(), _returnName);

            _cache.TryAdd(elementType, methodInfo);
            return methodInfo;
        }
        private static IEnumerable<T[]> PrefillBag<T>(int count, Func<T[]> factory)
        {
            int i = 0;
            while (i++ < count)
            {
                yield return factory();
            }
        }
        private static void ReturnArray(Array array, object pool, bool clearArray, NonLeaseablePool<object[]> bag, MethodInfo returnMethod)
        {
            object[] parameters = bag.Get();
            
            try
            {
                parameters[0] = array;
                parameters[1] = clearArray;
                _ = returnMethod.Invoke(pool, parameters);
            }
            finally
            {
                bag.Return(parameters);
            }
        }
    }
}

