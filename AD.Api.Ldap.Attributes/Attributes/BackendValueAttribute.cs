using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class BackendValueAttribute : Attribute
    {
        public string Value { get; }

        public BackendValueAttribute(string value)
        {
            this.Value = value;
        }
    }
}
