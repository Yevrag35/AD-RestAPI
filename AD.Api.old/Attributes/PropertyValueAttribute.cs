using System;
using System.DirectoryServices;
using System.Runtime.Versioning;

namespace AD.Api.Attributes
{
    [SupportedOSPlatform("windows")]
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyValueAttribute : Attribute
    {
        private static readonly Action<PropertyCollection, (string, object)> _clearAndAdd =
            (col, value) =>
            {
                var propValCol = col[value.Item1];
                if (null != propValCol && propValCol.Count > 0)
                    propValCol.Clear();

                propValCol.Add(value.Item2);
            };

        private static readonly Action<PropertyCollection, (string, object)> _set =
            (col, value) =>
            {
                col[value.Item1].Value = value.Item2;
            };

        public Action<PropertyCollection, (string, object)> Action { get; }

        public PropertyValueAttribute(AddMethod addMethod)
        {
            this.Action = GetAction(addMethod);
        }

        private static Action<PropertyCollection, (string, object)> GetAction(AddMethod method)
        {
            switch (method)
            {
                case AddMethod.ClearAndAdd:
                    return _clearAndAdd;

                default:
                    return _set;
            }
        }
    }
}
