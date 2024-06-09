using AD.Api.Ldap.Operations.Internal;
using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    public class Remove : EditPropertyOperationBase, ILdapOperationWithValues
    {
        public override OperationType OperationType => OperationType.Remove;
        public override string Property { get; }
        public List<object> Values { get; }

        public Remove(string propertyName)
            : base()
        {
            this.Property = propertyName;
            this.Values = new List<object>(1);
        }

        public override bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty)
        {
            this.HasBeenPerformed = true;
            if (collection.Count <= 0 || schemaProperty.IsSingleValued)
                return false;

            if (this.Values.Count <= 0)
                return true;

            this.Values.ForEach(obj =>
            {
                collection.Remove(obj);
            });

            this.Values.Clear();
            return true;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer)
        {
            string name = namingStrategy.GetPropertyName(this.Property, false);
            writer.WritePropertyName(name);

            writer.WriteStartArray();
            this.Values.ForEach(obj => serializer.Serialize(writer, obj));

            writer.WriteEndArray();
        }
    }
}
