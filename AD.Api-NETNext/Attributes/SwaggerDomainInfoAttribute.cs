namespace AD.Api.Attributes;

public sealed class SwaggerDomainInfoAttribute : Attribute
{
	public required string PropertyName { get; init; }
}