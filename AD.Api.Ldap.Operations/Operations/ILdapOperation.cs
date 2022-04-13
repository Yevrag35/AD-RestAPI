using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
