using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Requests
{
    public abstract class LdapRequest
    {
        /// <summary>
        /// The default timeout for LDAP requests.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> representing 30 seconds.
        /// </value>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        protected abstract DirectoryRequest BackingRequest { get; }
        protected virtual string DefaultRequestId => string.Empty;

        protected LdapRequest()
        {
        }

        public string GetRequestId()
        {
            return this.BackingRequest.RequestId;
        }
        public void Reset()
        {
            this.BackingRequest.Controls.Clear();
            this.BackingRequest.RequestId = this.DefaultRequestId;
            this.ResetCore();
        }
        protected abstract void ResetCore();
    }
}

