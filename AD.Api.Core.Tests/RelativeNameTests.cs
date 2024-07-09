using AD.Api.Core.Ldap;

namespace AD.Api.Core.Tests
{
    public class RelativeNameTests
    {
        [Theory]
        [InlineData("John Doe", RelativeNameType.CommonName, "CN=")]
        [InlineData("asdf", RelativeNameType.DomainComponent, "DC=")]
        [InlineData("asdf", RelativeNameType.OrganizationalUnit, "OU=")]
        [InlineData("asdf", RelativeNameType.Locality, "L=")]
        [InlineData("Doe\\, John", RelativeNameType.StreetAddress, "STREET=")]
        [InlineData("JOhn Doe", RelativeNameType.Country, "C=")]
        [InlineData("John Doe", RelativeNameType.StateOrProvince, "ST=")]
        public void RelativeName_CreateAddsPrefixWhenMissing(string dn, RelativeNameType type, string prefix)
        {
            RelativeName name = RelativeName.Create(dn, type);
            Assert.False(name.IsEmpty);
            Assert.Equal(type, name.AttributeType);
            Assert.NotEqual(dn, name.Value);
            Assert.Equal(dn.Length + prefix.Length, name.Value.Length);
            Assert.True(name.Value.AsSpan(0, prefix.Length).Equals(prefix, StringComparison.Ordinal));
            Assert.Equal(name.Value.Substring(prefix.Length), dn, ignoreCase: false);
        }
        [Theory]
        [InlineData("CN=John Doe", RelativeNameType.CommonName)]
        [InlineData("OU=Doe\\, John", RelativeNameType.OrganizationalUnit)]
        [InlineData("DC=contoso", RelativeNameType.DomainComponent)]
        [InlineData("ST=the place", RelativeNameType.StateOrProvince)]
        [InlineData("STREET=the streetz", RelativeNameType.StreetAddress)]
        [InlineData("UID=the guid", RelativeNameType.UserId)]
        [InlineData("O=the org", RelativeNameType.Organization)]
        [InlineData("L=the loc", RelativeNameType.Locality)]
        public void RelativeName_ParseAndDontAddPrefix(string dn, RelativeNameType type)
        {
            RelativeName name = RelativeName.Create(dn, type);
            Assert.False(name.IsEmpty);
            Assert.Equal(type, name.AttributeType);
            Assert.Equal(dn.Length, name.Value.Length);
            Assert.Equal(dn, name.Value);
        }

        [Theory]
        [InlineData("CN=John, Doe")]
        [InlineData("CN=John Doe ")]
        [InlineData("CN=Joh\"n\\, Doe")]
        [InlineData("CN;=John\\, Doe")]
        [InlineData("CN=J\\ohn\\, Doe")]
        [InlineData("CN=>John\\, Doe")]
        [InlineData("CN=John\\, Do<e")]
        [InlineData("CN=John\\, D+oe")]
        public void RelativeName_ThrowsOnUnescaped(string dn)
        {
            Assert.Throws<ArgumentException>(() => RelativeName.Create(dn, RelativeNameType.CommonName));
        }
        [Theory]
        [InlineData("CN=John\\, Doe")]
        [InlineData("CN=Joh\\\"nDoe")]
        [InlineData("CN=Joh\\; Doe")]
        [InlineData("CN=J\\\\ohn Doe")]
        [InlineData("CN=\\>JohnDoe")]
        [InlineData("CN=JohnDo\\<e")]
        [InlineData("CN=John D\\+oe")]
        public void RelativeName_DoesNotThrowOnEscaped(string dn)
        {
            RelativeName name = RelativeName.Create(dn, RelativeNameType.CommonName);
            Assert.False(name.IsEmpty);
            Assert.Equal(RelativeNameType.CommonName, name.AttributeType);
            Assert.Equal(dn.Length, name.Value.Length);
            Assert.Equal(dn, name.Value);
        }
    }
}
