using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace AD.Api.Core.Web.Extensions;

public static class ModelBindingContextExtensions
{
	public static string? GetFirstValue(this ModelBindingContext context)
	{
		return context.ValueProvider.GetValue(context.ModelName).FirstValue;
	}
	public static StringValues GetValues(this ModelBindingContext context)
	{
		return context.ValueProvider.GetValue(context.ModelName).Values;
	}
	public static bool IsValueRequired(this ModelBindingContext context)
	{
		ModelMetadata metadata = context.ModelMetadata;
		return metadata.IsBindingRequired || metadata.IsRequired;
	}
}