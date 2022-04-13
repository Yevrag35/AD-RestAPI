//using MG.Attributes;
using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BackendValueAttribute : Attribute
    {
        public string Value { get; }

        //object IValueAttribute.Value => this.Value;

        public BackendValueAttribute(string value)
        {
            this.Value = value;
        }

        //public T GetAs<T>()
        //{
        //    if (typeof(T).Equals(typeof(string)))
        //        return (T)((IValueAttribute)this).Value;

        //    else
        //        return (T)Convert.ChangeType(this.Value, typeof(T));
        //}

        //public bool ValueIsString() => true;
    }
}
