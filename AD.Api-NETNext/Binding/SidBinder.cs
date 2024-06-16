using AD.Api.Core.Security;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AD.Api.Binding
{
    public sealed class SidStringBinder : IModelBinder
    {
        private static readonly Type _strType = typeof(SidString);
        private static readonly ValidationStateEntry _suppress = new()
        {
            SuppressValidation = true,
        };
                

    public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);
            if (!_strType.Equals(bindingContext.ModelMetadata.UnderlyingOrModelType))
            {
                return Task.CompletedTask;
            }

            string? first = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
            if (string.IsNullOrWhiteSpace(first))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            try
            {
                SidString ss = new(first);
                var success = ModelBindingResult.Success(ss);
                bindingContext.Result = success;
                bindingContext.ValidationState[ss] = _suppress;
            }
            catch (Exception)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
