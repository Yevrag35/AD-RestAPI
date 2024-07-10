using AD.Api.Core.Web;
using Microsoft.AspNetCore.Http.Metadata;

namespace AD.Api.Binding.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FromQueryDNAttribute : LdapDistinguishedNameAttribute, IFromQueryMetadata
{
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FromRouteDNAttribute : LdapDistinguishedNameAttribute, IFromRouteMetadata
{
}