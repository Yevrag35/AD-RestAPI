using Newtonsoft.Json;
using System;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Exceptions;
using AD.Api.Extensions;
using AD.Api.Models.Collections;
using AD.Api.Models.Converters;

namespace AD.Api.Models.Exchange
{
    public class JsonExUser : JsonUser
    {
        [JsonProperty("targetAddress")]
        public string TargetAddress { get; set; }
    }
}
