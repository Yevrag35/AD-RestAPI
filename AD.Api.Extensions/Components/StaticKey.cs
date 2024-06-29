namespace AD.Api.Components
{
    public abstract class StaticKey : IEquatable<StaticKey>
    {
        public abstract bool Equals(StaticKey? other);
        public sealed override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            else if (obj is StaticKey key)
            {
                return this.Equals(key);
            }
            else
            {
                return false;
            }
        }
        public sealed override int GetHashCode()
        {
            return this.GetHashCodeCore();
        }

        protected abstract int GetHashCodeCore();

        public static bool operator ==(StaticKey? left, StaticKey? right)
        {
            return left?.Equals(right) ?? right is null;
        }
        public static bool operator !=(StaticKey? left, StaticKey? right)
        {
            return !(left == right);
        }

        public static StaticKey<T> Create<T>(T value) where T : notnull, IEquatable<T>
        {
            return new StaticKey<T>(value);
        }
    }
    public class StaticKey<T> : StaticKey, IEquatable<StaticKey<T>> where T : notnull, IEquatable<T>
    {
        private readonly T _value;
        private readonly int _hashCode;

        public StaticKey(T value)
        {
            _value = value;
            _hashCode = HashCode.Combine(Guid.NewGuid(), _value);
        }

        public sealed override bool Equals(StaticKey? other)
        {
            return ReferenceEquals(this, other);
        }
        public virtual bool Equals(StaticKey<T>? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            else if (other is null)
            {
                return false;
            }

            return _value.Equals(other._value);
        }
        protected override int GetHashCodeCore()
        {
            return _hashCode;
        }
    }
}

