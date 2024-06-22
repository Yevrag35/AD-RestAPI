using System;
using System.Collections.Concurrent;

namespace AD.Api.Pooling
{
    public class NonLeaseablePool<T> where T : class
    {
        private readonly ConcurrentBag<T> _bag;
        private int _count;
        private readonly Func<T> _factory;
        private readonly Action<T>? _reset;

        [MemberNotNullWhen(true, nameof(_reset))]
        internal bool HasReset { get; }
        public int MaxCapacity { get; }

        public NonLeaseablePool(int maxCapacity, IEnumerable<T>? collection, Func<T> factory, Action<T>? reset)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCapacity);
            this.MaxCapacity = maxCapacity;
            _factory = factory;
            _reset = reset;
            this.HasReset = reset is not null;
            _bag = collection is null
                ? new()
                : new(collection);
        }

        [return: NotNull]
        public T Get()
        {
            if (!_bag.TryTake(out T? item))
            {
                item = _factory();
            }
            else
            {
                _count--;
            }

            return item;
        }

        public void Return(T? item)
        {
            if (item is null)
            {
                return;
            }

            if (this.HasReset)
            {
                _reset(item);
            }

            if (_count < this.MaxCapacity)
            {
                _count++;
                _bag.Add(item);
            }
        }
    }
}

