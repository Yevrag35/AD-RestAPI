using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Requests;
using System.Text.Json.Serialization;

namespace AD.Api.Models;

public sealed class TestModel : IScopedRequest
{
    //[FromQueryDN(Name = "dn")]
    [JsonPropertyName("dn")]
    public required DistinguishedName DistinguishedName { get; init; }
    public int? Number { get; set; }

    public DistinguishedName GetScopedPath()
    {
        return this.DistinguishedName;
    }
    public string GetScopedPathMemberName()
    {
        return nameof(this.DistinguishedName);
    }
}