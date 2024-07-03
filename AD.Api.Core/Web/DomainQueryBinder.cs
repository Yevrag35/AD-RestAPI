using AD.Api.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Primitives;

namespace AD.Api.Binding
{
    public sealed class DomainQueryBinder : IModelBinder
    {
        private static readonly ValidationStateEntry _suppress = new()
        {
            SuppressValidation = true,
        };
        private static readonly Type _type = typeof(DomainQuery);

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);
            if (!_type.Equals(bindingContext.ModelMetadata.UnderlyingOrModelType))
            {
                return Task.CompletedTask;
            }

            HttpContext context = bindingContext.HttpContext;
            IQueryCollection query = context.Request.Query;

            string domain = GetQueryValue(query, DomainQuery.DomainModelName, string.Empty);
            string? dc = GetDomainControllerValue(query);
            try
            {
                ModelBindingResult success = DomainQuery.Create(domain, dc, context.RequestServices, out DomainQuery model);
                bindingContext.Result = success;
                bindingContext.ValidationState[model] = _suppress;
            }
            catch
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        private static string? GetDomainControllerValue(IQueryCollection query)
        {
            string? dc = GetQueryValue(query, DomainQuery.DomainControllerModelName, null);
            if (string.IsNullOrWhiteSpace(dc))
            {
                dc = GetQueryValue(query, DomainQuery.DomainControllerFullModelName, null);
            }

            return dc;
        }
        [return: NotNullIfNotNull(nameof(defaultValue))]
        private static string? GetQueryValue(IQueryCollection query, [ConstantExpected] string key, string? defaultValue)
        {
            return query.TryGetValue(key, out StringValues stringValues) && stringValues.Count > 0
                ? stringValues[0] ?? defaultValue
                : defaultValue;
        }
    }
}
