using AD.Api.Ldap.Operations;
using System;

namespace AD.Api.Ldap.Exceptions
{
    public class LdapOperationgParsingException : Exception
    {
        private const string COMMENT = "Unable to parse the specified LDAP operation.";

        /// <summary>
        /// The keyword that caused the <see cref="LdapOperationgParsingException"/>.
        /// </summary>
        public OperationType? OffendingKeyword { get; }

        public LdapOperationgParsingException()
            : base(COMMENT)
        {
        }

        public LdapOperationgParsingException(string message)
            : base(message)
        {
        }

        public LdapOperationgParsingException(string message, OperationType keyword)
            : base(message)
        {
            this.OffendingKeyword = keyword;
        }
    }
}
