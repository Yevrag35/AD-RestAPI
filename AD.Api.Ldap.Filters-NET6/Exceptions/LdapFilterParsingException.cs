using AD.Api.Ldap.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Exceptions
{
    /// <summary>
    /// An class indicating that an fatal <see cref="Exception"/> occurred while serializing or deserializing an LDAP filter.
    /// </summary>
    public class LdapFilterParsingException : Exception
    {
        private const string COMMENT = "Unable to parse the specified LDAP filter.";

        /// <summary>
        /// The keyword that caused the <see cref="LdapFilterParsingException"/>.
        /// </summary>
        public FilterType? OffendingKeyword { get; }

        public LdapFilterParsingException()
            : base(COMMENT)
        {
        }

        public LdapFilterParsingException(string message)
            : base(message)
        {
        }

        public LdapFilterParsingException(string message, FilterType keyword)
            : base(message)
        {
            this.OffendingKeyword = keyword;
        }
    }
}
