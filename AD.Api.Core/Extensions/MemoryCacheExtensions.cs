using Microsoft.Extensions.Caching.Memory;

namespace AD.Api.Core.Extensions;

public static class MemoryCacheExtensions
{
	public static bool TryGetUnsafeValue<TKey, TValue>(this IMemoryCache cache, TKey key, [NotNullWhen(true)] out TValue? value)
		where TKey : notnull
		where TValue : class
	{
		if (cache.TryGetValue(key, out object? obj))
		{
			value = Unsafe.As<TValue>(obj)!;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
	public static bool TryGetValueOrRemove<TKey, TValue>(this IMemoryCache cache, TKey key, [NotNullWhen(true)] out TValue? value) where TKey : notnull
	{
		if (cache.TryGetValue(key, out TValue? obj))
		{
			if (obj is not null)
			{
				value = obj;
				return true;
			}

			cache.Remove(key);
		}

		value = default;
		return false;
	}
}

