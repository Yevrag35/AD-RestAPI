using AD.Api.Attributes.Services;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AD.Api.Core.Web.Validation
{
    //[DependencyRegistration(typeof(IModelValidator), Lifetime = ServiceLifetime.Singleton)]
    public sealed class ScopeAuthorizationValidator : IModelValidator, IModelValidatorProvider
    {
        private static ModelValidationResult[] BuildErrorResult(string? parentPath, in AuthorizedRole requiredRole)
        {
            ModelValidationResult[] results = new ModelValidationResult[2];
            string message = string.Format(Messages.JWT_Unauthorized, parentPath);
            string roleMsg = string.Format(Messages.JWT_Unauthorized_RequiredRole, requiredRole.ToString());

            results[0] = new ModelValidationResult(null, message);
            results[1] = new ModelValidationResult(null, roleMsg);
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

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            if (typeof(IScopedRequest).IsAssignableFrom(context.ModelMetadata.ContainerType))
            {
                context.Results.Add(new ValidatorItem
                {
                    Validator = this,
                    IsReusable = true,
                });
            }
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (context.Container is not IScopedRequest scopedRequest
                ||
                !scopedRequest.GetScopedPathMemberName().Equals(context.ModelMetadata.Name, StringComparison.OrdinalIgnoreCase))
            {
                return [];
            }

            DistinguishedName scopedDn = scopedRequest.GetScopedPath();

            ModelValidationResult[] results = [];

            if (IsNotScopedAuthorized(in scopedDn, context.ActionContext.HttpContext, out string? parentPath, out AuthorizedRole requiredRole))
            {
                results = BuildErrorResult(parentPath, in requiredRole);
            }

            return results;
        }
    }
}
