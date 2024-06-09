using AD.Api.Ldap.Operations;

namespace AD.Api.Ldap.Exceptions
{
    public class LdapOperationParsingException : Exception
    {
        private const string COMMENT = "Unable to parse the specified LDAP operation.";

        /// <summary>
        /// The keyword that caused the <see cref="LdapOperationParsingException"/>.
        /// </summary>
        public OperationType? OffendingKeyword { get; }

        public LdapOperationParsingException()
            : base(COMMENT)
        {
        }

        public LdapOperationParsingException(string message)
            : base(message)
        {
        }

        public LdapOperationParsingException(string message, OperationType keyword)
            : base(message)
        {
            this.OffendingKeyword = keyword;
        }
    }
}
