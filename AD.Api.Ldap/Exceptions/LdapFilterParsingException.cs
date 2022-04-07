using AD.Api.Ldap.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Exceptions
{
    public class LdapFilterParsingException : Exception
    {
        private const string COMMENT = "Unable to parse the specified LDAP filter.";

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
