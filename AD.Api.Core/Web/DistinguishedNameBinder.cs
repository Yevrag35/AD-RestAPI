using AD.Api.Attributes;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Enums;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Buffers;

namespace AD.Api.Core.Web;

public interface IDistinguishedNameAttribute
{
    RelativeNameType[] AllowedTypes { get; }
    bool IsRequired { get; }
}

public sealed class DistinguishedNameBinder : IModelBinder
{
    private const string DN_ERROR_KEY = "distinguishedName";
    private const string FORBIDDEN_KEY = "forbidden";
    private static readonly ValidationStateEntry _suppress = new()
    {
        SuppressValidation = true,
    };
    private static readonly Type _type = typeof(DistinguishedName);
    private static readonly ModelBindingResult _successEmpty = ModelBindingResult.Success(DistinguishedName.Empty);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        if (!_type.Equals(bindingContext.ModelMetadata.UnderlyingOrModelType))
        {
            return Task.CompletedTask;
        }

        IEnumValues<RelativeNameType, BackendValueAttribute, string>? enumValues = TryGetAttribute(bindingContext, out IDistinguishedNameAttribute? attribute, out DefaultModelMetadata? defaultMetadata)
            ? GetEnumValues(bindingContext)
            : null;

        ReadOnlySpan<char> first = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (first.IsWhiteSpace())
        {
            if (ValueIsRequired(bindingContext, attribute))
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else if (bindingContext.ModelMetadata.IsNullableValueType)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }

        int roughCount = first.Count(CharConstants.COMMA) + 1;

        RelativeName[] array = ArrayPool<RelativeName>.Shared.Rent(roughCount);
        SpanStringBuilder erroredSections = new(first.Length);

        if (!DistinguishedName.TrySplit(first, array, ref erroredSections, out int written))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            Span<char> chars = erroredSections.AsSpan();
            if (!chars.IsEmpty)
            {
                bindingContext.ActionContext.ModelState.AddModelError(DN_ERROR_KEY, "The following sections are invalid:");
                foreach (ReadOnlySpan<char> section in chars.SpanSplit(Environment.NewLine))
                {
                    bindingContext.ActionContext.ModelState.AddModelError(DN_ERROR_KEY, section.ToString());
                }
            }
            else
            {
                bindingContext.ActionContext.ModelState.AddModelError(DN_ERROR_KEY, "The distinguished name is invalid.");
            }

            ArrayPool<RelativeName>.Shared.Return(array);
            erroredSections.Dispose();
            return Task.CompletedTask;
        }

        DistinguishedName dn = new(array.AsSpan(0, written));
        erroredSections.Dispose();
        ArrayPool<RelativeName>.Shared.Return(array);

        if (!IsScopeAuthorized(dn, bindingContext.HttpContext, out string? parentPath, out AuthorizedRole requiredRole))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            string message = string.Format(Messages.JWT_Unauthorized, parentPath);
            string roleMsg = string.Format(Messages.JWT_Unauthorized_RequiredRole, requiredRole.ToString());

            bindingContext.ActionContext.ModelState.AddModelError(FORBIDDEN_KEY, message);
            bindingContext.ActionContext.ModelState.AddModelError(FORBIDDEN_KEY, roleMsg);
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(dn);
        return Task.CompletedTask;
    }

    private static IEnumValues<RelativeNameType, BackendValueAttribute, string> GetEnumValues(ModelBindingContext context)
    {
        return context
            .HttpContext
                .RequestServices
                    .GetRequiredService<IEnumValues<RelativeNameType, BackendValueAttribute, string>>();
    }
    private static bool IsScopeAuthorized(DistinguishedName dn, HttpContext context, [NotNullWhen(false)] out string? parentPath, out AuthorizedRole requiredRole)
    {
        IAuthorizer authorizer = context.RequestServices.GetRequiredService<IAuthorizer>();
        if (!authorizer.IsAuthorized(context, dn, out requiredRole))
        {
            parentPath = dn.GetParent();
            return false;
        }
        else
        {
            parentPath = null;
            return true;
        }
    }
    private static bool TryGetAttribute(ModelBindingContext context, [NotNullWhen(true)] out IDistinguishedNameAttribute? attribute, [NotNullWhen(true)] out DefaultModelMetadata? metadata)
    {
        metadata = context.ModelMetadata as DefaultModelMetadata;
        if (metadata is null)
        {
            attribute = null;
            return false;
        }

        attribute = metadata
            .Attributes
                .Attributes
                    .OfType<IDistinguishedNameAttribute>()
                    .FirstOrDefault();

        return attribute is not null;
    }
    private static bool ValueIsRequired(ModelBindingContext context, IDistinguishedNameAttribute? attribute)
    {
        ModelMetadata metadata = context.ModelMetadata;
        return metadata.IsBindingRequired
               ||
               metadata.IsRequired
               ||
               (attribute is not null && attribute.IsRequired);
    }
}
