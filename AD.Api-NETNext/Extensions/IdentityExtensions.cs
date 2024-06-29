using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;

namespace AD.Api.Extensions
{
    public static class IdentityExtensions
    {
        [DebuggerStepThrough]
        public static bool IsAuthenticated(this HttpContext context)
        {
            return IsAuthenticated(context.User);
        }
        [DebuggerStepThrough]
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return IsAuthenticated(principal.Identity);
        }
        [DebuggerStepThrough]
        public static bool IsAuthenticated([NotNullWhen(true)] this IIdentity? identity)
        {
            return identity?.IsAuthenticated ?? false;
        }

        [DebuggerStepThrough]
        public static bool TryGetWindowsIdentity(this HttpContext context, [NotNullWhen(true)] out WindowsIdentity? identity)
        {
            return TryGetWindowsIdentity(context.User, out identity);
        }
        [DebuggerStepThrough]
        public static bool TryGetWindowsIdentity(this ClaimsPrincipal principal, [NotNullWhen(true)] out WindowsIdentity? identity)
        {
            return TryGetWindowsIdentity(principal.Identity, out identity);
        }
        [DebuggerStepThrough]
        public static bool TryGetWindowsIdentity([NotNullWhen(true)] this IIdentity? identity, [NotNullWhen(true)] out WindowsIdentity? windowsIdentity)
        {
            if (IsAuthenticated(identity) && identity is WindowsIdentity wid)
            {
                windowsIdentity = wid;
                return true;
            }
            else
            {
                windowsIdentity = null;
                return false;
            }
        }
    }
}
