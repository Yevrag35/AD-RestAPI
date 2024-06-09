using AD.Api.Ldap.Path;

namespace AD.Api.Ldap.Models
{
    public interface IPathed
    {
        PathValue? Path { get; }
    }
}
