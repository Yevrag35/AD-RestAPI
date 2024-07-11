using AD.Api.Serialization.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Requests;

public sealed class MoveRequest : IJsonOnDeserialized, IValidatableObject
{
    [PrivateExtensionData]
    private JsonDictionary? _extensionData;

    [MinLength(1, ErrorMessage = "New names must be at least 1 character in length.")]
    public string? NewName { get; set; }

    [Required]
    [JsonRequired]
    public required DistinguishedName NewParentDn { get; init; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (string.IsNullOrWhiteSpace(this.NewName) && _extensionData is not null
            &&
            _extensionData.TryGetValue("name", out object? nameObj)
            &&
            nameObj is string newName)
        {
            this.NewName = newName;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.NewParentDn.IsEmpty)
        {
            yield return new ValidationResult("The new parent distinguished name must not be empty.", [nameof(this.NewParentDn)]);
        }
        else if (this.NewParentDn.Count <= 1)
        {
            yield return new ValidationResult("The new parent distinguished name must have at least 2 components.", [nameof(this.NewParentDn)]);
        }
    }
}