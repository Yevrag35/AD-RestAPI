using AD.Api.Ldap.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Converters
{
    public class FileTimeConverter : LdapPropertyConverter<DateTime?>
    {
        public override DateTime? Convert(LdapPropertyAttribute attribute, object[]? rawValue, DateTime? existingValue, bool hasExistingValue)
        {
            if (rawValue is null || rawValue.Length <= 0)
            {
                return null;
            }

            if (rawValue[0] is not long fileTime)
            {
                return null;
            }

            return DateTime.FromFileTime(fileTime);
        }
    }
}
