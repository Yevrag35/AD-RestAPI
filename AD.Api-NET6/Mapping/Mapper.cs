using AD.Api.Ldap.Extensions;
using AutoMapper;
using Newtonsoft.Json;

namespace AD.Api.Mapping
{
    public class Mapper : Profile
    {
        //private static readonly ValueConverter _valueConverter = new ValueConverter();

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

        //private class ValueConverter : ITypeConverter<IList<JsonConverter>, JsonConverterCollection>, IValueConverter<IList<JsonConverter>, JsonConverterCollection>
        //{
        //    public JsonConverterCollection Convert(IList<JsonConverter> source, JsonConverterCollection destination, ResolutionContext context)
        //    {
        //        for (int i = 0; i < source.Count; i++)
        //        {
        //            destination.Add(source[i]);
        //        }

        //        return destination;
        //    }

        //    public JsonConverterCollection Convert(IList<JsonConverter> sourceMember, ResolutionContext context)
        //    {
        //        return this.Convert(sourceMember, new JsonConverterCollection(), context);
        //    }
        //}
    }
}