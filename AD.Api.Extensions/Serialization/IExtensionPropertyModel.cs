namespace AD.Api.Serialization;

public interface IExtensionPropertyModel
{
    [return: NotNullIfNotNull(nameof(propertyName))]
    string? GetAlternateName(string? propertyName);

    [return: NotNullIfNotNull(nameof(propertyName))]
    public string? GetFaultingProperty(string? propertyName);
}