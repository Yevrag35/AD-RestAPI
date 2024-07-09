using AD.Api.Core.Ldap;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace AD.Api.Binding.Attributes
{
    public sealed class FromQueryDNAttribute : Attribute, IBinderTypeProviderMetadata, IDistinguishedNameAttribute, IFromQueryMetadata, IModelNameProvider
    {
        private static readonly Type _type = typeof(DistinguishedNameBinder);

        public RelativeNameType[] AllowedTypes { get; } = [];
        public bool IsRequired { get; init; } = true;
        public string? Name { get; init; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type? IBinderTypeProviderMetadata.BinderType => _type;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        BindingSource? IBindingSourceMetadata.BindingSource => BindingSource.Custom;

        public FromQueryDNAttribute()
        {
        }
    }
}
