using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AD.Api.Core.Security.Accounts
{
    public interface IAccountName : IValidatableObject
    {
        void SetCredential(NetworkCredential credential);
    }
}

