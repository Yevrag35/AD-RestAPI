using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore)]
    public sealed class QueryResult
    {
        private readonly string _host = string.Empty;
        private readonly ICollection _results = Array.Empty<string>();

        [JsonProperty(Order = 2, Required = Required.Always)]
        public int Count => this.Results.Count;

        [JsonProperty(Order = 0, Required = Required.Always)]
        [NotNull]
        public string? Host
        {
            get => _host;
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                _host = value;
            }
        }

        [JsonProperty(Order = 1)]
        public QueryResultDetails? Request { get; set; }

        [JsonProperty(Order = 3, Required = Required.Always)]
        [NotNull]
        public ICollection? Results
        {
            get => _results;
            init
            {
                if (value is null)
                {
                    return;
                }

                _results = value;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class QueryResultDetails
    {
        private readonly string _filter = string.Empty;
        private readonly IList<string> _properties = Array.Empty<string>();

        [JsonProperty(Order = 0)]
        [NotNull]
        public string? FilterUsed
        {
            get => _filter;
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                _filter = value;
            }
        }

        [JsonProperty(Order = 1)]
        public int Limit { get; init; }

        [JsonProperty(Order = 2)]
        public int PropertyCount => _properties.Count;

        [JsonProperty(Order = 3)]
        [NotNull]
        public IList<string>? PropertiesRequested
        {
            get => _properties;
            init
            {
                if (value is null)
                {
                    return;
                }

                _properties = value;
            }
        }
    }
}
