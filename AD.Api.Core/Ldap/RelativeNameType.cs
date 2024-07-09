using AD.Api.Attributes;

namespace AD.Api.Core.Ldap;

/// <summary>
/// Typical Relative Distinguished Name (RDN) attribute types.
/// </summary>
public enum RelativeNameType
{
    /// <summary>
    /// No attribute type is specified.
    /// </summary>
    None,
    /// <summary>
    /// The common name attribute: <c>CN</c>
    /// </summary>
    /// <remarks>
    /// Also used as the default when no attribute type is specified.
    /// </remarks>
    [BackendValue("CN=")]
    CommonName = 0x1,
    /// <summary>
    /// The organizational unit name attribute: <c>OU</c>
    /// </summary>
    [BackendValue("OU=")]
    OrganizationalUnit = 0x2,
    /// <summary>
    /// The domain component attribute: <c>DC</c>
    /// </summary>
    [BackendValue("DC=")]
    DomainComponent = 0x3,
    /// <summary>
    /// The organization name attribute: <c>O</c>
    /// </summary>
    [BackendValue("O=")]
    Organization = 0x4,
    /// <summary>
    /// The street address attribute: <c>STREET</c>
    /// </summary>
    [BackendValue("STREET=")]
    StreetAddress = 0x5,
    /// <summary>
    /// The locality name attribute: <c>L</c>
    /// </summary>
    [BackendValue("L=")]
    Locality = 0x6,
    /// <summary>
    /// The state or province name attribute: <c>ST</c>
    /// </summary>
    [BackendValue("ST=")]
    StateOrProvince = 0x7,
    /// <summary>
    /// The country name attribute: <c>C</c>
    /// </summary>
    [BackendValue("C=")]
    Country = 0x8,
    /// <summary>
    /// The user ID attribute: <c>UID</c>
    /// </summary>
    [BackendValue("UID=")]
    UserId = 0x9,
}
