using Microsoft.Extensions.ObjectPool;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
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
        /// <inheritdoc cref="IResettable.TryReset" path="/*[not(self::returns)]"/>
        public void Reset()
        {
            this.BackingRequest.Controls.Clear();
            this.BackingRequest.RequestId = this.DefaultRequestId;
            this.ResetCore();
        }
        /// <summary>
        /// When overriden in a derived class, performs any request-specific reset operations.
        /// </summary>
        /// 
        protected abstract void ResetCore();

        public static implicit operator DirectoryRequest(LdapRequest request)
        {
            return request.BackingRequest;
        }
    }
}

