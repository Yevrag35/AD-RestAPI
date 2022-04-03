using Newtonsoft.Json;
using System;
using AD.Api.Components;
using AD.Api.Extensions;
using AD.Api.Models.Converters;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Exceptions
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class IllegalADOperationException : InvalidOperationException
    {
        [JsonProperty("message", Order = 1)]
        public string Msg => base.Message;

        [JsonProperty("operator", Order = 2)]
        [JsonConverter(typeof(CamelStringEnumConverter))]
        public Operation OffendingOperation { get; private set; }

        public IllegalADOperationException(string reason, Operation operation)
            : base(Strings.Exception_IllegalOp_Format.Format(reason))
        {
            this.OffendingOperation = operation;
        }
    }
}
