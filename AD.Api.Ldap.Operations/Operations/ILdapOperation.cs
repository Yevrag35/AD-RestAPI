using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    public interface ILdapOperation
    {
        bool HasBeenPerformed { get; }
        OperationType OperationType { get; }
        string Property { get; }

        bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty);
        void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer);
    }
}
