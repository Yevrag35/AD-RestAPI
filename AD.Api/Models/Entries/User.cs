using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Attributes;
using AD.Api.Components;

namespace AD.Api.Models.Entries
{
    [SupportedOSPlatform("windows")]
    public class User : EntryBase<User>
    {
        [Ldap("postaladdress")]
        [PropertyValue(AddMethod.Set)]
        public string Address { get; set; }

        [Ldap("admindescription")]
        [PropertyValue(AddMethod.Set)]
        public string AdminDescription { get; set; }

        [Ldap("c")]
        [PropertyValue(AddMethod.Set)]
        public string Country { get; set; } = "US";

        [Ldap("co")]
        [PropertyValue(AddMethod.Set)]
        public string Co { get; set; } = "United States";

        [Ldap("cn")]
        [PropertyValue(AddMethod.Set)]
        public string CommonName { get; set; }

        [Ldap("countrycode")]
        [PropertyValue(AddMethod.Set)]
        public int? CountryCode { get; set; } = 840;

        [Ldap("description")]
        [PropertyValue(AddMethod.Set)]
        public string Description { get; set; }

        [Ldap("department")]
        [PropertyValue(AddMethod.Set)]
        public string Department { get; set; }

        [Ldap("displayname")]
        [PropertyValue(AddMethod.Set)]
        public string DisplayName { get; set; }

        [Ldap("mail")]
        [PropertyValue(AddMethod.Set)]
        public string EmailAddress { get; set; }

        [Ldap("employeeid")]
        [PropertyValue(AddMethod.Set)]
        public string EmployeeId { get; set; }

        [Ldap("employeetype")]
        [PropertyValue(AddMethod.Set)]
        public string EmployeeType { get; set; }

        [Ldap("givenname")]
        [PropertyValue(AddMethod.Set)]
        public string GivenName { get; set; }

        [Ldap("homedirectory")]
        [PropertyValue(AddMethod.Set)]
        public string HomeDirectory { get; set; }

        [Ldap("homedrive")]
        [PropertyValue(AddMethod.Set)]
        public string HomeDrive { get; set; }

        [Ldap("initials")]
        [PropertyValue(AddMethod.Set)]
        public string Initials { get; set; }

        [Ldap("l")]
        [PropertyValue(AddMethod.Set)]
        public string Location { get; set; }

        [Ldap("manager")]
        [PropertyValue(AddMethod.Set)]
        public string Manager { get; set; }

        [Ldap("mstsallowlogon")]
        [PropertyValue(AddMethod.Set)]
        public bool? MsTsAllowLogon { get; set; }

        [Ldap("mstshomedirectory")]
        [PropertyValue(AddMethod.Set)]
        public string MsTsHomeDirectory { get; set; }

        [Ldap("mstshomedrive")]
        [PropertyValue(AddMethod.Set)]
        public string MsTsHomeDrive { get; set; }

        [Ldap("name")]
        [PropertyValue(AddMethod.Set)]
        public string Name { get; set; }

        [Ldap("physicaldeliveryofficename")]
        [PropertyValue(AddMethod.Set)]
        public string Office { get; set; }

        [Ldap("proxyaddresses")]
        [PropertyValue(AddMethod.Add)]
        public LdapStringList ProxyAddresses { get; set; }

        [Ldap("samaccountname")]
        [PropertyValue(AddMethod.Set)]
        public string SamAccountName { get; set; }

        [Ldap("st")]
        [PropertyValue(AddMethod.Set)]
        public string State { get; set; }

        [Ldap("street")]
        [PropertyValue(AddMethod.Set)]
        public string Street { get; set; }

        [Ldap("streetaddress")]
        [PropertyValue(AddMethod.Set)]
        public string StreetAddress { get; set; }

        [Ldap("sn")]
        [PropertyValue(AddMethod.Set)]
        public string Surname { get; set; }

        [Ldap("telephonenumber")]
        [PropertyValue(AddMethod.Set)]
        public string Telephone { get; set; }

        [Ldap("title")]
        [PropertyValue(AddMethod.Set)]
        public string Title { get; set; }

        [Ldap("userprincipalname")]
        [PropertyValue(AddMethod.Set)]
        public string UserPrincipalName { get; set; }

        [Ldap("postalcode")]
        [PropertyValue(AddMethod.Set)]
        public string ZipCode { get; set; }

        public User()
            : base()
        {
        }
    }
}
