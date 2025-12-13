using AD.Api.Core.Web;

namespace AD.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class LdapRequiresSSLAttribute : Attribute, ILdapRequireSSLMetadata
{
	bool ILdapRequireSSLMetadata.IsForced => true;
}
