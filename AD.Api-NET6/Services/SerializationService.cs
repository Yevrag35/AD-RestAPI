using AD.Api.Ldap.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AD.Api.Services
{
    public interface ISerializationService
    {
        void Prepare(IJsonSerializable? serializable);
        void PrepareMany<T>(IEnumerable<T>? collection) where T : IJsonSerializable;
    }

    public class SerializationService : ISerializationService
    {
        private readonly MvcNewtonsoftJsonOptions _options;

        private IMapper Mapper { get; }
        private JsonSerializerSettings Settings => _options.SerializerSettings;

        public SerializationService(IMapper mapper, IOptions<MvcNewtonsoftJsonOptions> options)
        {
            this.Mapper = mapper;
            _options = options.Value;
        }

        private JsonSerializerSettings CreateNew()
        {
            return this.Mapper.Map<JsonSerializerSettings>(this.Settings);
        }

        private void Prepare(IJsonSerializable? serializable, JsonSerializerSettings settings)
        {
            serializable?.PrepareForSerialization(settings);
        }

        public void Prepare(IJsonSerializable? serializable)
        {
            if (serializable is null)
                return;

            this.Prepare(serializable, this.CreateNew());
        }
        public void PrepareMany<T>(IEnumerable<T>? collection) where T : IJsonSerializable
        {
            if (collection is null)
                return;

            JsonSerializerSettings settings = this.CreateNew();

            foreach (var serializable in collection)
            {
                this.Prepare(serializable, settings);
            }
        }
    }
}
