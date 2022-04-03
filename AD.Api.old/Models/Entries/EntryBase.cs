using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Proxies;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Extensions;

namespace AD.Api.Models.Entries
{
    [SupportedOSPlatform("windows")]
    public abstract class EntryBase<T> : Entry, IDirObject
    {
        private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;

        public EntryBase()
            : base()
        {
            this.Attributes = new EntryAttributeDictionary();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainController"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"/>
        public DirectoryEntry GetDirectoryEntry(string domainController = null)
        {
            if (string.IsNullOrWhiteSpace(this.DistinguishedName))
                throw new InvalidOperationException("DistinguishedName cannot be null or empty.");

            string ldapPath = this.DistinguishedName.ToLdapPath(domainController);
            return new DirectoryEntry(ldapPath);
        }

        public IReadOnlyList<(string, object)> GetValues()
        {
            IReadOnlyList<(PropertyInfo, string)> pis = this.GetNonNullProperties();
            (string, object)[] values = new (string, object)[pis.Count];

            for (int i = 0; i < pis.Count; i++)
            {
                (PropertyInfo, string) item = pis[i];
                values[i] = (item.Item2, item.Item1.GetValue(this));
            }

            return values;
        }

        public IReadOnlyList<(PropertyInfo, string)> GetNonNullProperties()
        {
            PropertyInfo[] props = this.GetType().GetProperties(FLAGS);
            return props.Where(x =>
                x.GetCustomAttributes<LdapAttribute>().Any()
                &&
                null != x.GetValue(this))
                .Select(x =>
                    (x, x.GetCustomAttribute<LdapAttribute>().Name))
                .ToArray();
        }
        public IReadOnlyList<(PropertyInfo, string, object)> GetLdapProperties()
        {
            PropertyInfo[] props = this.GetType().GetProperties(FLAGS);
            return props.Where(x =>
                x.GetCustomAttributes<LdapAttribute>().Any()
                &&
                null != x.GetValue(this))
                .Select(x =>
                    (x, x.GetCustomAttribute<LdapAttribute>().Name, x.GetValue(this)))
                .ToArray();
        }
    }
}
