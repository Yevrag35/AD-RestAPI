using System;

namespace AD.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class BackendValueAttribute : AdApiAttribute, IValuedAttribute<string>
    {
        public string Value { get; }

        public BackendValueAttribute(string value)
        {
            this.Value = value;
        }
    }
}
