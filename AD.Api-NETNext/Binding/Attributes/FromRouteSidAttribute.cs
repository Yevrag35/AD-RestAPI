using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Binding.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class FromRouteSidAttribute : ModelBinderAttribute, IFromRouteMetadata
    {
        private static readonly Type _type = typeof(SidStringBinder);

        public FromRouteSidAttribute()
            : base(_type)
        {
        }
    }
}
