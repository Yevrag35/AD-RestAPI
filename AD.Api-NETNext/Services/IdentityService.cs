using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IIdentityService
    {
        //SafeAccessTokenHandle DuplicateAccessToken(SafeAccessTokenHandle disposedToken);
        bool TryGetKerberosIdentity(ClaimsPrincipal? claimsPrincipal, [NotNullWhen(true)] out WindowsIdentity? windowsIdentity);
    }

    public class IdentityService : IIdentityService
    {
        public IdentityService()
        {
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="accessToken"></param>
        ///// <returns></returns>
        ///// <exception cref="SecurityException"></exception>
        //public SafeAccessTokenHandle DuplicateAccessToken(SafeAccessTokenHandle accessToken)
        //{
        //    if (accessToken.IsInvalid)
        //        return accessToken;

        //    SafeAccessTokenHandle duplicateAccessToken = SafeAccessTokenHandle.InvalidHandle;
        //    IntPtr currentProcessHandle = GetCurrentProcess();

        //    if (!DuplicateHandle(
        //        currentProcessHandle,
        //        accessToken,
        //        currentProcessHandle,
        //        ref duplicateAccessToken,
        //        0u,
        //        true,
        //        DUPLICATE_SAME_ACCESS))
        //    {
        //        throw new SecurityException(new Win32Exception().Message);
        //    }

        //    return duplicateAccessToken;
        //}

        public bool TryGetKerberosIdentity(ClaimsPrincipal? claimsPrincipal, [NotNullWhen(true)] out WindowsIdentity? windowsIdentity)
        {
            windowsIdentity = null;

            if (claimsPrincipal?.Identity is WindowsIdentity wid 
                &&
                wid.IsAuthenticated 
                &&
                !string.IsNullOrWhiteSpace(wid.AuthenticationType)
                &&
                wid.AuthenticationType.Equals(NegotiateDefaults.AuthenticationScheme, StringComparison.CurrentCultureIgnoreCase)
                &&
                !wid.AccessToken.IsClosed
                &&
                !wid.AccessToken.IsInvalid)
            {
                windowsIdentity = wid;
            }

            return windowsIdentity is not null;
        }

        //private const uint DUPLICATE_SAME_ACCESS = 0x00000002;

        //[DllImport("kernel32.dll")]
        //private static extern IntPtr GetCurrentProcess();

        //[DllImport("kernel32.dll")]
        //private static extern bool DuplicateHandle(
        //    IntPtr hSourceProcessHandle,
        //    SafeAccessTokenHandle hSourceHandle,
        //    IntPtr hTargetProcessHandle,
        //    ref SafeAccessTokenHandle lpTargetHandle,
        //    uint dwDesiredAccess,
        //    bool bInheritHandle,
        //    uint dwOptions);
    }
}
