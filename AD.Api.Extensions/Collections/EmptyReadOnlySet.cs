using AD.Api.Collections.Exceptions;
using System.Collections;

namespace AD.Api.Collections
{
    /// <summary>
    /// A <see langword="static"/> class for creating empty, read-only sets with caching mechanisms similar to 
    /// <see cref="Array.Empty{T}"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static class EmptyReadOnlySet
    {
        private static readonly Lazy<TypeSetDictionary> _cache = new();

        /// <summary>
        /// Returns an empty, read-only set of the specified type that implements <see cref="ISet{T}"/>.
        /// </summary>
        /// <typeparam name="T">
        /// <inheritdoc cref="ISet{T}" path="/typeparam"/>
        /// </typeparam>
        /// <returns>
        /// A new or cached empty, read-only set of the specified type that implements <see cref="ISet{T}"/>.
        /// </returns>
        public static ISet<T> Get<T>()
        {
            return GetOrAdd<T>();
        }
        /// <summary>
        /// Returns an empty, read-only set of the specified type that implements <see cref="IReadOnlySet{T}"/>.
        /// </summary>
        /// <typeparam name="T">
        /// <inheritdoc cref="IReadOnlySet{T}" path="/typeparam"/>
        /// </typeparam>
        /// <returns>
        /// A new or cached empty, read-only set of the specified type that implements <see cref="IReadOnlySet{T}"/>.
        /// </returns>
        public static IReadOnlySet<T> GetReadOnly<T>()
        {
            return GetOrAddReadOnly<T>();
        }

        private static ISet<T> GetOrAdd<T>()
        {
            if (!_cache.Value.TryGetValue(out ISet<T>? emptySet, out Type type))
            {
                emptySet = CreateNew<T>(type);
            }

            return emptySet;
        }
        private static IReadOnlySet<T> GetOrAddReadOnly<T>()
        {
            if (!_cache.Value.TryGetValue(out IReadOnlySet<T>? emptySet, out Type type))
            {
                emptySet = CreateNewReadOnly<T>(type);
            }

            return emptySet;
        }
        private static ISet<T> CreateNew<T>(Type type)
        {
            ISet<T> newSet = new EmptyReadOnlySet<T>();
            _cache.Value.Add(newSet, type);
            return newSet;
        }
        private static IReadOnlySet<T> CreateNewReadOnly<T>(Type type)
        {
            IReadOnlySet<T> newSet = new EmptyReadOnlySet<T>();
            _cache.Value.Add(newSet, type);
            return newSet;
        }

        [DebuggerStepThrough]
        private sealed class TypeSetDictionary : UnsafeDictionary<Type>
        {
            internal TypeSetDictionary() : base() { }

            internal bool Add<T>(IReadOnlySet<T> readOnlySet, Type type)
            {
                return this.TryAdd(type, readOnlySet);
            }
            internal bool Add<T>(ISet<T> set, Type type)
            {
                return this.TryAdd(type, set);
            }

            internal bool TryGetValue<T>([NotNullWhen(true)] out ISet<T>? set, out Type typeSearched)
            {
                typeSearched = typeof(T);
                return this.TryGetValue(typeSearched, out set);
            }
            internal bool TryGetValue<T>([NotNullWhen(true)] out IReadOnlySet<T>? readOnlySet, out Type typeSearched)
            {
                typeSearched = typeof(T);
                return this.TryGetValue(typeSearched, out readOnlySet);
            }
        }
    }

    [DebuggerStepThrough]
    file readonly struct EmptyReadOnlySet<T> : IReadOnlySet<T>, ISet<T>
    {
        public readonly int Count => 0;
        public readonly bool IsReadOnly => true;

        public readonly bool Add(T item)
        {
            return false;
        }
        readonly void ICollection<T>.Add(T item)
        {
            throw new ReadOnlyException();
        }

        readonly void ICollection<T>.Clear()
        {
            Debug.Fail("Cannot clear a read-only, empty set.");
            return;
        }
        public readonly bool Contains(T item)
        {
            return false;
        }
        readonly void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            return;
        }
        public readonly void ExceptWith(IEnumerable<T> other)
        {
            return;
        }

        public readonly IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public readonly void IntersectWith(IEnumerable<T> other)
        {
            return;
        }

        public readonly bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return false;
        }

        public readonly bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return false;
        }

        public readonly bool IsSubsetOf(IEnumerable<T> other)
        {
            return (this.TryGetNonEnumeratedCount(out int count) && count == 0) || !other.Any();
        }

        public readonly bool IsSupersetOf(IEnumerable<T> other)
        {
            return (this.TryGetNonEnumeratedCount(out int count) && count == 0) || !other.Any();
        }

        public readonly bool Overlaps(IEnumerable<T> other)
        {
            return false;
        }

        public readonly bool Remove(T item)
        {
            return false;
        }

        public readonly bool SetEquals(IEnumerable<T> other)
        {
            return (this.TryGetNonEnumeratedCount(out int count) && count == 0) || !other.Any();
        }

        public readonly void SymmetricExceptWith(IEnumerable<T> other)
        {
            return;
        }

        public readonly void UnionWith(IEnumerable<T> other)
        {
            if ((other.TryGetNonEnumeratedCount(out int count) && count == 0) || !other.Any())
            {
                return;
            }

            throw new ReadOnlyException();
        }
    }
}

