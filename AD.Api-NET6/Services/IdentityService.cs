using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IIdentityService
    {
        bool TryGetKerberosIdentity(ClaimsPrincipal? claimsPrincipal, [NotNullWhen(true)] out WindowsIdentity? windowsIdentity);
    }

    public class IdentityService : IIdentityService
    {
        public IdentityService()
        {
        }

        public bool TryGetKerberosIdentity(ClaimsPrincipal? claimsPrincipal, [NotNullWhen(true)] out WindowsIdentity? windowsIdentity)
        {
            windowsIdentity = null;

            if (claimsPrincipal?.Identity is WindowsIdentity wid 
                &&
                wid.IsAuthenticated 
                &&
                !string.IsNullOrWhiteSpace(wid.AuthenticationType)
                &&
                wid.AuthenticationType.Equals(NegotiateDefaults.AuthenticationScheme, StringComparison.CurrentCultureIgnoreCase))
            {
                windowsIdentity = wid;
            }

            return windowsIdentity is not null;
        }
    }
}
