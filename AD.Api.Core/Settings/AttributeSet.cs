using AD.Api.Core.Serialization;
using System.Collections;
using System.Collections.Frozen;

namespace AD.Api.Core.Settings
{
    /// <summary>
    /// A read-only set of attribute names that are to be treated as a specified <see cref="Type"/> when serializing
    /// and deserializing.
    /// </summary>
    public abstract class AttributeSet : IReadOnlySet<string>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IReadOnlySet<string> _attributes;

        public int Count => _attributes.Count;
        public abstract SerializerAction SerializerAction { get; }
        public abstract Type SerializationType { get; }

        [DebuggerStepThrough]
        protected private AttributeSet()
        {
            _attributes = FrozenSet<string>.Empty;
        }
        [DebuggerStepThrough]
        protected private AttributeSet(string[] attributes)
        {
            _attributes = new HashSet<string>(attributes, StringComparer.OrdinalIgnoreCase);
        }

        public static AttributeSet<T> Create<T>(IConfigurationSection configurationSection, SerializerAction action)
        {
            return AttributeSet<T>.Create(configurationSection, action);
        }

        [DebuggerStepThrough]
        public bool Contains(string item)
        {
            return _attributes.Contains(item);
        }
        [DebuggerStepThrough]
        public bool IsProperSubsetOf(IEnumerable<string> other)
        {
            return _attributes.IsProperSubsetOf(other);
        }
        [DebuggerStepThrough]
        public bool IsProperSupersetOf(IEnumerable<string> other)
        {
            return _attributes.IsProperSupersetOf(other);
        }
        [DebuggerStepThrough]
        public bool IsSubsetOf(IEnumerable<string> other)
        {
            return _attributes.IsSubsetOf(other);
        }
        [DebuggerStepThrough]
        public bool IsSupersetOf(IEnumerable<string> other)
        {
            return _attributes.IsSupersetOf(other);
        }

        [DebuggerStepThrough]
        public bool Overlaps(IEnumerable<string> other)
        {
            return _attributes.Overlaps(other);
        }
        [DebuggerStepThrough]
        public bool SetEquals(IEnumerable<string> other)
        {
            return _attributes.SetEquals(other);
        }

        [DebuggerStepThrough]
        public IEnumerator<string> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// A read-only set of attribute names that are to be treated as <typeparamref name="T"/> when serializing 
    /// and deserializing.
    /// </summary>
    /// <remarks>
    /// During serialization, the attribute value(s) are converted to <typeparamref name="T"/>.
    /// During deserialization, the string representation is converted back to <typeparamref name="T"/> and possibly 
    /// back to an array.
    /// </remarks>
    public abstract class AttributeSet<T> : AttributeSet
    {
        public sealed override SerializerAction SerializerAction { get; }
        public sealed override Type SerializationType { get; }

        protected private AttributeSet(SerializerAction action) : base()
        {
            this.SerializerAction = action;
            this.SerializationType = typeof(T);
        }
        protected private AttributeSet(string[] attributes, SerializerAction action) : base(attributes)
        {
            this.SerializerAction = action;
            this.SerializationType = typeof(T);
        }

        [DebuggerStepThrough]
        private sealed class EmptyAttributesSet : AttributeSet<T>
        {
            public EmptyAttributesSet(SerializerAction action) : base(action) { }
        }
        [DebuggerStepThrough]
        private sealed class NonEmptyAttributeSet : AttributeSet<T>
        {
            internal NonEmptyAttributeSet(string[] attributes, SerializerAction action) : base(attributes, action)
            {
            }
        }

        internal static AttributeSet<T> Create(IConfigurationSection guidAttributesSection, SerializerAction serializerAction)
        {
            string[] attributes = guidAttributesSection.Exists()
                ? guidAttributesSection.Get<string[]>(x => x.ErrorOnUnknownConfiguration = false) ?? []
                : [];

            return attributes.Length > 0
                ? new NonEmptyAttributeSet(attributes, serializerAction)
                : new EmptyAttributesSet(serializerAction);
        }
    }
}

