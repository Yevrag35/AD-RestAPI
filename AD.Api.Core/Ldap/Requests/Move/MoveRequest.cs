using AD.Api.Core.Serialization;
using AD.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Ldap.Requests;

public sealed class MoveRequest : RequestExtensionModel
{
    private const string OTHER_NAME = "name";
    private const string OTHER_PARENT = "newParent";
    private static readonly string[] _extraKeys = [OTHER_NAME, OTHER_PARENT];

    //[MinLength(1, ErrorMessage = "New names must be at least 1 character in length.")]
    [UserPrincipalName]
    public string? NewName { get; set; }

    [DistinguishedName(RequiredRelativeNameType = RelativeNameType.OrganizationalUnit)]
    public DistinguishedName? NewParentDn { get; set; }

    public MoveRequest() : base(_extraKeys)
    {
    }

    protected override void OnDeserialized(IReadOnlyDictionary<string, object?> extensionData)
    {
        if (extensionData.Count == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(this.NewName) && extensionData.TryGetValue(OTHER_NAME, out object? nameObj)
            &&
            nameObj is string newName)
        {
            this.NewName = newName;
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
    //public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //    if (string.IsNullOrWhiteSpace(this.NewName))
    //    {
    //        yield return new ValidationResult("The new name must not be null, empty, or whitespace.",
    //            this.GetFaultingProperty<MoveRequest>(x => x.NewName, OTHER_NAME));
    //    }

    //    if (!this.NewParentDn.HasValue || this.NewParentDn.Value.IsEmpty)
    //    {
    //        yield return new ValidationResult("The new parent distinguished name must not be empty.", 
    //            this.GetFaultingProperty<MoveRequest>(x => x.NewParentDn, OTHER_PARENT));
    //    }
    //    else if (this.NewParentDn.Value.Count <= 1)
    //    {
    //        yield return new ValidationResult("The new parent distinguished name must have at least 2 components.", 
    //            this.GetFaultingProperty<MoveRequest>(x => x.NewParentDn, OTHER_PARENT));
    //    }
    //}
}