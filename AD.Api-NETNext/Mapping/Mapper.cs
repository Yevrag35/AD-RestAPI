using AD.Api.Ldap.Extensions;
using AutoMapper;
using Newtonsoft.Json;

namespace AD.Api.Mapping
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            this.CreateMap<JsonSerializerSettings, JsonSerializerSettings>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ForMember(x => x.Converters, mem =>
                {
                    mem.Ignore();
                })
                .AfterMap((source, dest) =>
                {
                    AddConverters(source.Converters, dest.Converters);
                });
        }

        private static void AddConverters(IList<JsonConverter> source, IList<JsonConverter> dest)
        {
            source.ForEach(converter =>
            {
                dest.Add(converter);
            });
        }
    }
}