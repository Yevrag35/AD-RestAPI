using AD.Api.Core.Ldap.Requests;
using AD.Api.Validation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Ldap.Users;

public sealed class UserMoveRequest : MoveRequest
{
    [NotNull]
    [RequiredAfterDeserialization]
    [RelativeName(RequiredType = RelativeNameType.CommonName)]
    public override RelativeName? NewName
    {
        [DebuggerStepThrough]
        get => base.NewName;
        [DebuggerStepThrough]
        set => base.NewName = value;
    }
    [NotNull]
    [RequiredAfterDeserialization]
    [DistinguishedName(MinimumSegmentCount = 2)]
    public override DistinguishedName? NewParentDn
    {
        [DebuggerStepThrough]
        get => base.NewParentDn;
        [DebuggerStepThrough]
        set => base.NewParentDn = value;
    }
}