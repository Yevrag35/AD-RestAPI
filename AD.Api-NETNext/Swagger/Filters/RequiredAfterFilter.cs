using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AD.Api.Swagger.Filters;

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