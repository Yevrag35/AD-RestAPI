using AD.Api.Attributes.Services;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AD.Api.Core.Web.Validation
{
    [DependencyRegistration(typeof(IModelValidator), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class ScopeAuthorizationValidator : IModelValidator
    {
        private static ModelValidationResult[] BuildErrorResult(string memberName, string? parentPath, in AuthorizedRole requiredRole)
        {
            ModelValidationResult[] results = new ModelValidationResult[2];
            string message = string.Format(Messages.JWT_Unauthorized, parentPath);
            string roleMsg = string.Format(Messages.JWT_Unauthorized_RequiredRole, requiredRole.ToString());

            results[0] = new ModelValidationResult(memberName, message);
            results[1] = new ModelValidationResult(memberName, roleMsg);
            return results;
        }

        private static bool IsNotScopedAuthorized(in DistinguishedName dn, HttpContext context, [NotNullWhen(true)] out string? parentPath, out AuthorizedRole requiredRole)
        {
            IAuthorizer authorizer = context.RequestServices.GetRequiredService<IAuthorizer>();
            if (authorizer.IsAuthorized(context, dn, out requiredRole))
            {
                parentPath = null;
                return false;
            }
            else
            {
                parentPath = dn.GetParent();
                return true;
            }
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (context.Model is not IScopedRequest scopedRequest)
            {
                return [];
            }

            DistinguishedName scopedDn = scopedRequest.GetScopedPath();

            ModelValidationResult[] results = [];

            if (IsNotScopedAuthorized(in scopedDn, context.ActionContext.HttpContext, out string? parentPath, out AuthorizedRole requiredRole))
            {
                string memberName = scopedRequest.GetScopedPathMemberName();
                results = BuildErrorResult(memberName, parentPath, in requiredRole);
            }

            return results;
        }
    }
}
