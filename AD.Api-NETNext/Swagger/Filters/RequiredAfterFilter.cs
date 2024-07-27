using AD.Api.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AD.Api.Swagger.Filters
{
    public sealed class RequiredAfterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in context.ApiDescription.ParameterDescriptions)
            {
                if (BindingSource.Body == parameter.ParameterDescriptor.BindingInfo?.BindingSource)
                {

                }
            }
        }
    }
}