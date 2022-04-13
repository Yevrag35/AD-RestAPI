using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    public abstract class EditPropertyOperationBase : ILdapOperation
    {
        public bool HasBeenPerformed { get; protected set; }
        public abstract OperationType OperationType { get; }
        public abstract string Property { get; }

        public abstract bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty);

        protected bool Modify(PropertyValueCollection collection, SchemaProperty schemaProperty, object value)
        {
            int count = 0;
            if (value is int intVal)
                count = intVal;

            else if (value is string strVal)
                count = strVal.Length;

            if ((schemaProperty.RangeLower.HasValue &&
                count.CompareTo(schemaProperty.RangeLower.Value) < 0)
                ||
                (schemaProperty.RangeUpper.HasValue &&
                count.CompareTo(schemaProperty.RangeUpper) > 0))
            {
                throw new ArgumentOutOfRangeException($"{value} is outside of the range allowed by the attribute.");
            }

            collection.Value = value;
            return true;
        }

        public abstract void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer);
    }
}