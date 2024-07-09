using AD.Api.Core.Ldap;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.Api.Core.Web;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class LdapDistinguishedNameAttribute : Attribute, IBinderTypeProviderMetadata, IDistinguishedNameAttribute, IModelNameProvider
{
    private static readonly Type _type = typeof(DistinguishedNameBinder);

    public RelativeNameType[] AllowedTypes { get; } = [];
    public bool IsRequired { get; init; } = true;
    public string? Name { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Type? BinderType { get; }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public BindingSource? BindingSource { get; }

    public LdapDistinguishedNameAttribute()
        : this(BindingSource.Custom)
    {
    }
    protected LdapDistinguishedNameAttribute(BindingSource bindingSource)
    {
        this.BinderType = _type;
        this.BindingSource = bindingSource;
    }
}