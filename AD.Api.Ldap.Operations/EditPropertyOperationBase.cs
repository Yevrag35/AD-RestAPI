using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    public abstract class EditPropertyOperationBase
    {
        public bool HasBeenPerformed { get; protected set; }
        public abstract OperationType OperationType { get; }
        public abstract string Property { get; }

        public abstract bool Perform(PropertyValueCollection collection);

        public abstract void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer);
    }
}