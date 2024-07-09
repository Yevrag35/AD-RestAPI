using AD.Api.Core.Ldap;

namespace AD.Api.Core.Tests
{
    public sealed class DistinguishedNameTests
    {
        [Theory]
        [InlineData("CN=John Doe,OU=Users,DC=contoso,DC=com", 4, RelativeNameType.CommonName)]
        [InlineData("CN=Doe\\, Doe,OU=Users,DC=contoso,DC=com", 4, RelativeNameType.CommonName)]
        [InlineData("OU=Users,DC=contoso,DC=com", 3, RelativeNameType.OrganizationalUnit)]
        [InlineData("DC=contoso,DC=com", 2, RelativeNameType.DomainComponent)]
        [InlineData("O=The Company,DC=contoso,DC=com", 3, RelativeNameType.Organization)]
        [InlineData("UID=incomplete", 1, RelativeNameType.UserId)]
        [InlineData("UID=John Doe\\ ,OU=Users,DC=contoso,DC=com", 4, RelativeNameType.UserId)]
        [InlineData("UID=\\#John Doe,OU=Users,DC=contoso,DC=com", 4, RelativeNameType.UserId)]
        public void DistinguishedName_ParsesCorrectly(string dn, int nameCount, RelativeNameType type)
        {
            DistinguishedName name = DistinguishedName.Parse(dn);
            Assert.False(name.IsEmpty);
            Assert.Equal(nameCount, name.Count);
            Assert.Equal(type, name.Type);
        }

        [Theory]
        [InlineData("CN=John Doe,OU=Users,DC=contoso,DC=com")]
        [InlineData("CN=Doe\\, Doe,OU=Users,DC=contoso,DC=com")]
        [InlineData("OU=Users,DC=contoso,DC=com")]
        [InlineData("DC=contoso,DC=com")]
        [InlineData("O=The Company,DC=contoso,DC=com")]   
        [InlineData("UID=incomplete")]
        [InlineData("UID=John Doe\\ ,OU=Users,DC=contoso,DC=com")]
        [InlineData("UID=\\#John Doe,OU=Users,DC=contoso,DC=com")]
        public void DistinguishedName_ToStringEqualsInput(string input)
        {
            DistinguishedName dn = DistinguishedName.Parse(input);
            Assert.Equal(input, dn.ToString());
        }

        [Fact]
        public void DistinguishedName_ParsesEmptyIfEmpty()
        {
            DistinguishedName dn = DistinguishedName.Parse(string.Empty);
            Assert.Equal(string.Empty, dn.ToString());
            Assert.True(dn.IsEmpty);
            Assert.Empty(dn);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(4)]
        public void DistinguishedName_ParsesEmptyIfWhitespace(int howManySpaces)
        {
            string spaces = new(' ', howManySpaces);
            DistinguishedName dn = DistinguishedName.Parse(spaces);
            Assert.NotEqual(spaces, dn.ToString());
            Assert.True(dn.IsEmpty);
            Assert.Empty(dn);
        }

        [Theory]
        [InlineData("CN=the guy,OU=Users,DC=contoso,DC=com", "OU=Users,DC=contoso,DC=com")]
        public void DistinguishedName_GetParent_ReturnsCorrectParent(string dn, string parent)
        {
            DistinguishedName dName = DistinguishedName.Parse(dn);
            string gotParent = dName.GetParent();
            Assert.Equal(parent, gotParent);
        }
        [Theory]
        [InlineData("DC=contoso,DC=com")]
        [InlineData("CN=contoso")]
        [InlineData("OU=the OU")]
        [InlineData("")]
        public void DistinguishedName_GetParent_DomainSingleOrEmptyReturnsEmpty(string dn)
        {
            DistinguishedName dName = DistinguishedName.Parse(dn);
            string gotParent = dName.GetParent();
            Assert.NotNull(gotParent);
            Assert.Equal(string.Empty, gotParent);
        }

        [Theory]
        [InlineData("DC=contoso,DC=com", 2)]
        [InlineData("CN=contoso", 1)]
        [InlineData("CN=John Doe,OU=Users,DC=contoso,DC=com", 4)]
        [InlineData("UID=Doe\\, Doe,OU=Users,DC=contoso,DC=com", 4)]
        [InlineData("OU=Users,DC=contoso,DC=com", 3)]
        [InlineData("O=The Company,DC=contoso,DC=com", 3)]
        public void DistinguishedName_CountRelativeNames_ReturnsCorrectCount(string dn, int expectedCount)
        {
            int count = DistinguishedName.CountNumberOfRelativeNames(dn);
            Assert.Equal(expectedCount, count);
            bool tried = DistinguishedName.TryCountNumberOfRelativeNames(dn, out count);
            Assert.True(tried);
            Assert.Equal(expectedCount, count);
        }

        [Fact]
        public void DistinguishedName_TryCountRelativeNamesReturnsTrueOnEmpty()
        {
            bool tried = DistinguishedName.TryCountNumberOfRelativeNames(string.Empty, out int count);
            Assert.True(tried);
            Assert.Equal(0, count);
        }
        [Fact]
        public void DistinguishedName_TryCountRelativeNamesReturnsFalseOnInvalid()
        {
            bool tried = DistinguishedName.TryCountNumberOfRelativeNames("CNakasi3h3894r7,,DCq12=a", out int count);
            Assert.False(tried);
        }
        //[Theory]
        //[InlineData("DC=contoso")]
        //[InlineData("DC=com")]
        //[InlineData("OU=theOU")]
        //[InlineData("CN=the guy")]
        //[InlineData("CN=guy\\, the")]
        //public void DistinguishedName_ParseEmptyIfOneComponent(string dn)
        //{
        //    DistinguishedName dName = DistinguishedName.Parse(dn);
        //    Assert.True(dName.IsEmpty);
        //    Assert.NotEqual(dn, dName.ToString());
        //    Assert.Empty(dName);
        //}
    }
}
