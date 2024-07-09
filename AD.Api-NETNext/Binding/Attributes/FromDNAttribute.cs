using AD.Api.Core.Web;
using Microsoft.AspNetCore.Http.Metadata;

namespace AD.Api.Binding.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FromQueryDNAttribute : DistinguishedNameAttribute, IFromQueryMetadata
{
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FromRouteDNAttribute : DistinguishedNameAttribute, IFromRouteMetadata
{
}