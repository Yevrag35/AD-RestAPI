using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization.Json.Ldap;

public interface IJsonAttributeConverter
{
    IAttributeConverter Converter { get; }

    void SetConverter(IAttributeConverter converter);
}

public abstract class AttributeOperationConverterBase<T> : JsonConverter<T>, IJsonAttributeConverter
{
    public IAttributeConverter Converter { get; private set; }

    protected AttributeOperationConverterBase()
    {
        this.Converter = null!;
    }

    void IJsonAttributeConverter.SetConverter(IAttributeConverter converter)
    {
        this.Converter = converter;
    }
}