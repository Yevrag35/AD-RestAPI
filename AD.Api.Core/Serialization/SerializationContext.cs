using AD.Api.Attributes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AD.Api.Core.Serialization
{
    public ref struct SerializationContext
    {
        private readonly IServiceProvider _provider;
        private ReadOnlySpan<char> _attributeName;
        private readonly JsonSerializerOptions _options;
        private object _value;

        public ReadOnlySpan<char> AttributeName
        {
            readonly get => _attributeName;
            internal set => _attributeName = value;
        }
        public readonly JsonSerializerOptions Options => _options;
        public object Value
        {
            readonly get => _value;
            internal set => _value = value;
        }

        internal SerializationContext(JsonSerializerOptions options, IServiceProvider scopedProvider)
        {
            _attributeName = default;
            _provider = scopedProvider;
            _options = options;
            _value = string.Empty;
        }

        public readonly object? GetService(Type serviceType)
        {
            return _provider.GetService(serviceType);
        }
        public readonly T GetRequiredService<T>() where T : class
        {
            return _provider.GetRequiredService<T>();
        }
    }
}

