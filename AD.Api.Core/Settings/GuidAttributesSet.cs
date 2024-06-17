using System.Collections;
using System.Collections.Frozen;

namespace AD.Api.Core.Settings
{
    /// <summary>
    /// A read-only set of attribute names that are to be treated as GUIDs when serializing and deserializing.
    /// </summary>
    /// <remarks>
    /// During serialization, the attribute value's byte array is converted to a GUID.
    /// During deserialization, the string representation is converted back to a GUID and possibly back to a byte array.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    public abstract class GuidAttributesSet : IReadOnlySet<string>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly FrozenSet<string> _attributes;

        public int Count => _attributes.Count;

        [DebuggerStepThrough]
        protected GuidAttributesSet()
        {
            _attributes = FrozenSet<string>.Empty;
        }
        [DebuggerStepThrough]
        protected GuidAttributesSet(string[] attributes)
        {
            _attributes = attributes.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }

        [DebuggerStepThrough]
        private sealed class EmptyGuidAttributesSet : GuidAttributesSet
        {
            public EmptyGuidAttributesSet() : base() { }
        }
        [DebuggerStepThrough]
        private sealed class AttributeSet : GuidAttributesSet
        {
            internal AttributeSet(string[] attributes) : base(attributes)
            {
            }
        }

        public static GuidAttributesSet Create(IConfigurationSection guidAttributesSection)
        {
            string[] attributes = guidAttributesSection.Exists()
                ? guidAttributesSection.Get<string[]>(x => x.ErrorOnUnknownConfiguration = false) ?? []
                : [];

            return attributes.Length > 0
                ? new AttributeSet(attributes)
                : new EmptyGuidAttributesSet();
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
}

