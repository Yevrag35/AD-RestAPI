using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Schema
{
    public class SchemaProperty : IEquatable<SchemaProperty>
    {
        private readonly int? _lower;
        private readonly int? _upper;

        public Guid Class { get; init; }

        [MemberNotNullWhen(true, nameof(RangeLower), nameof(RangeUpper))]
        public bool HasRange => this.RangeLower.HasValue && this.RangeUpper.HasValue;

        public bool IsInGlobalCatalog { get; init; } = true;
        public bool IsSingleValued { get; init; }
        public string Name { get; init; } = string.Empty;
        public int? RangeLower
        {
            get => _lower;
            init
            {
                _lower = value;
            }
        }
        public int? RangeUpper
        {
            get => _upper;
            init
            {
                _upper = value;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is SchemaProperty sp && this.Equals(sp);
        }
        public bool Equals(SchemaProperty? other)
        {
            if (other is null)
                return false;

            else if (ReferenceEquals(this, other))
                return true;

            else
                return this.Class.Equals(other.Class)
                    && StringComparer.CurrentCultureIgnoreCase.Equals(this.Name, other.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Class, this.Name.ToUpper());
        }
    }
}
