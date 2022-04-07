using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Recurs : EqualityStatement
    {
        private const string TRANSITIVE_EVAL = "{0}:1.2.840.113556.1.4.1941:";

        public sealed override string Property { get; }
        public sealed override FilterType Type => FilterType.Recurse | FilterType.Equal;
        public string Value { get; }

        public Recurs(string propertyName, string distinguishedName)
            : base()
        {
            this.Property = string.Format(TRANSITIVE_EVAL, propertyName);
            this.Value = distinguishedName ?? throw new ArgumentNullException(nameof(distinguishedName));
        }

        [return: NotNull]
        protected internal sealed override string? GetValue()
        {
            return this.Value;
        }
        protected override EqualityStatement ToAny()
        {
            return this;
        }
    }
}
