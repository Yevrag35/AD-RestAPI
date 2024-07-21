using AD.Api.Core.Serialization;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Ldap.Requests;

public abstract class MoveRequest : RequestExtensionModel
{
    private const string OTHER_NAME = "name";
    private const string OTHER_PARENT = "newParent";
    private static readonly string[] _extraKeys = [OTHER_NAME, OTHER_PARENT];
    private static readonly string NEW_NAME = nameof(NewName);
    private static readonly string NEW_PARENT_DN = nameof(NewParentDn);

    [NotNull]
    public virtual RelativeName? NewName { get; set; }

    [NotNull]
    public virtual DistinguishedName? NewParentDn { get; set; }

    protected MoveRequest() : base(_extraKeys)
    {
    }

    [return: NotNullIfNotNull(nameof(propertyName))]
    public override string? GetAlternateName(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        if (NEW_NAME.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
        {
            return OTHER_NAME;
        }
        else if (NEW_PARENT_DN.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
        {
            return OTHER_PARENT;
        }
        else
        {
            return propertyName;
        }
    }
    protected override void OnDeserialized(IReadOnlyDictionary<string, object?> extensionData)
    {
        if (extensionData.Count == 0)
        {
            return;
        }

        if (!this.NewName.HasValue && extensionData.TryGetValue(OTHER_NAME, out object? nameObj)
            &&
            nameObj is string newName)
        {
            this.NewName = RelativeName.TryParse(newName, RelativeNameType.CommonName, out RelativeName rdn)
                ? rdn
                : RelativeName.Empty;
        }

        if ((!this.NewParentDn.HasValue || this.NewParentDn.Value.IsEmpty)
            &&
            extensionData.TryGetValue(OTHER_PARENT, out object? newParentObj)
            &&
            newParentObj is string newParent)
        {
            this.NewParentDn = DistinguishedName.Parse(newParent);
        }
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return [];
    }
}