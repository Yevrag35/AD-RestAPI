using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Enums;
using Microsoft.AspNetCore.Http;

namespace AD.Api.Core.Extensions;

public static class HttpContextExtensions
{
    public static void AddNeedsScoping(this HttpContext context, in AuthorizedRole role)
    {
        string roleName = context.RequestServices.GetRequiredService<IEnumStrings<AuthorizedRole>>()[role];
        context.Items[AuthorizationScope.NeedsScoping] = roleName;
    }
    public static AuthorizationScope[] GetScopes(this HttpContext context)
    {
        return context.Items.TryGetValue(AuthorizationScope.CLAIM_TYPE, out object? value) && value is AuthorizationScope[] scopes
            ? scopes
            : [];
    }
    public static bool NeedsScoping(this HttpContext context, out AuthorizedRole requiredRole)
    {
        if (context.Items.TryGetValue(AuthorizationScope.NeedsScoping, out object? value)
            &&
            value is string roleString
            &&
            context.RequestServices.GetRequiredService<IEnumStrings<AuthorizedRole>>().TryGetEnum(roleString, out requiredRole))
        {
            return true;
        }

        requiredRole = AuthorizedRole.None;
        return false;
    }
    public static bool RemoveNeedsScoping(this HttpContext context)
    {
        return context.Items.Remove(AuthorizationScope.NeedsScoping);
    }
    public static bool TryGetScopes(this HttpContext context, [NotNullWhen(true)] out AuthorizationScope[]? scopes)
    {
        if (context.Items.TryGetValue(AuthorizationScope.CLAIM_TYPE, out object? value) && value is AuthorizationScope[] authScopes)
        {
            scopes = authScopes;
            return true;
        }
        else
        {
            scopes = null;
            return false;
        }
    }
}