using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AD.Api.Ldap.Operations
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class EditOperationRequest
    {
        [JsonIgnore]
        public ClaimsPrincipal? ClaimsPrincipal { get; set; }

        /// <summary>
        /// The DistinguishedName of the object to edit.
        /// </summary>
        /// <example>CN=Doe\, John,OU=CorpUsers,DC=contoso,DC=com</example>
        [JsonProperty("dn", Order = 1, Required = Required.Always)]
        [Required]
        public string DistinguishedName { get; private set; } = string.Empty;

        /// <summary>
        /// The domain that object resides in.
        /// </summary>
        [JsonIgnore]
        public string? Domain { get; set; } = string.Empty;

        /// <summary>
        /// All edit operations to perform on the object.
        /// </summary>
        /// <example>
        /// {
        ///     "remove": {
        ///         "proxyAddresses": [
        ///             "smtp:john.doe2@contoso.com",
        ///             "smtp:john.doe@contoso.net"
        ///         ]
        ///     "set": {
        ///         "userAccountControl": 514
        ///     }
        /// }
        /// </example>
        [JsonProperty("edits", Order = 2)]
        public List<ILdapOperation> EditOperations { get; private set; } = new List<ILdapOperation>(1);
    }
}
