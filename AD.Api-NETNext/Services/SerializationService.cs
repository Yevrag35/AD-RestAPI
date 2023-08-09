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
        void Prepare(IJsonPreparable? serializable);
        void PrepareMany<T>(IEnumerable<T>? collection) where T : IJsonPreparable;
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

        private static void Prepare(IJsonPreparable? serializable, JsonSerializerSettings settings)
        {
            serializable?.PrepareForSerialization(settings);
        }

        public void Prepare(IJsonPreparable? serializable)
        {
            if (serializable is null)
            {
                return;
            }

            Prepare(serializable, this.CreateNew());
        }
        public void PrepareMany<T>(IEnumerable<T>? collection) where T : IJsonPreparable
        {
            if (collection is null)
            {
                return;
            }

            JsonSerializerSettings settings = this.CreateNew();

            foreach (var serializable in collection)
            {
                Prepare(serializable, settings);
            }
        }
    }
}
