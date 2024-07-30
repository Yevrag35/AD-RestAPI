using AD.Api.Core.Operations;
using AD.Api.Exceptions;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Ldap.Requests;

public sealed class AccountStatusUpdateRequest : IEditOperation
{
    private readonly bool _toggle;
    private UserAccountControl _newUac;

    public AccountStatusUpdateRequest(bool toggle)
    {
        _newUac = default;
        _toggle = toggle;
    }

    public void ApplyToRequest(ModifyRequest request)
    {
        if (default == _newUac)
        {
            return;
        }

        DirectoryAttributeModification modification = new()
        {
            Name = AttributeConstants.USER_ACCOUNT_CONTROL,
            Operation = DirectoryAttributeOperation.Replace
        };

        ref int number = ref Unsafe.As<UserAccountControl, int>(ref _newUac);

        _ = modification.Add(number.ToString());
        _ = request.Modifications.Add(modification);
    }
    public void ReadChangeFromCurrent(SearchResultEntry currentEntry)
    {
        if (!currentEntry.Attributes.Contains(AttributeConstants.USER_ACCOUNT_CONTROL))
        {
            throw new AdApiException("Search result entry does not contain the user account control attribute.");
        }

        DirectoryAttribute uacAttribute = currentEntry.Attributes[AttributeConstants.USER_ACCOUNT_CONTROL];
        if (uacAttribute[0] is not string uacNumString || !int.TryParse(uacNumString, out int uacNumber))
        {
            throw new AdApiException("User account control attribute value is not a valid number.");
        }

        UserAccountControl uac = (UserAccountControl)uacNumber;
        if (_toggle)
        {
            uac &= ~UserAccountControl.Disabled;
        }
        else
        {
            uac = uac |= UserAccountControl.Disabled;
        }

        _newUac = uac;
    }
}