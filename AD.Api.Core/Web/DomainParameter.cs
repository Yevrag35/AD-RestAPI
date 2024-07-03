using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.Api.Binding.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DomainAttribute : Attribute, IBinderTypeProviderMetadata
{
    private static readonly Type _binderType = typeof(DomainQueryBinder);

    public Type BinderType => _binderType;
    public BindingSource BindingSource => BindingSource.Custom;

    public DomainAttribute()
    {
    }
}
