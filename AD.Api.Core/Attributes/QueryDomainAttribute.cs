using Microsoft.AspNetCore.Http.Metadata;

namespace AD.Api.Binding.Attributes;

/// <summary>
/// An attribute decorating a parameter or property to indicate that the value should be
/// bound from the query string using the key <c>domain</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property,
    AllowMultiple = false, Inherited = true)]
public sealed class QueryDomainAttribute : Attribute, IFromQueryMetadata
{
    public const string ModelName = "domain";

    [NotNull]
    public string? Name => ModelName;

    public QueryDomainAttribute()
    {
    }
}
