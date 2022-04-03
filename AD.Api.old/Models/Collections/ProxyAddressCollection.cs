using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models.Collections
{
    public class ProxyAddressCollection : ADValueSet<string>
    {
        private const StringComparison _ignoreCase = StringComparison.CurrentCultureIgnoreCase;
        private static readonly Predicate<string> _primaryClause = address => address.StartsWith(Strings.SMTP_Prefix_Primary);

        public ProxyAddressCollection()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }

        public ProxyAddressCollection(int capacity)
            : base(capacity, StringComparer.CurrentCultureIgnoreCase)
        {
        }

        public ProxyAddressCollection(IEnumerable<string> addresses)
            : base(addresses, StringComparer.CurrentCultureIgnoreCase)
        {
        }

        public bool ContainsPrimary()
        {
            return this.InnerList.Exists(_primaryClause);
        }
        public string GetPrimary()
        {
            return this.InnerList.Find(_primaryClause);
        }

        protected override bool Add(string item, bool adding)
        {
            //return DoListFunction(item, s => base.Add(s, true));
            item = CheckPrefix(item);
            return base.Add(item, adding);
        }

        protected override bool Contains(string item, bool querying)
        {
            //return DoListFunction(item, s => base.Contains(s, true));
            item = CheckPrefix(item, false);
            return base.Contains(item, querying);
        }

        protected override int IndexOf(string item, bool querying)
        {
            //return DoListFunction(item, s => base.IndexOf(item, true));
            item = CheckPrefix(item, false);
            return base.IndexOf(item, querying);
        }

        protected override void Insert(int index, string item, bool inserting)
        {
            //DoListAction(item, (x) => base.Insert(index, x));
            item = CheckPrefix(item);
            base.Insert(index, item, inserting);
        }

        protected override bool Remove(string item, bool removing)
        {
            //return DoListFunction(item, s => base.Remove(s, true));
            item = CheckPrefix(item, false);
            return base.Remove(item, removing);
        }

        protected override bool ReplaceValueAtIndex(int index, string newValue)
        {
            //return DoListFunction(newValue, s => base.ReplaceValueAtIndex(index, s));
            newValue = CheckPrefix(newValue);
            return base.ReplaceValueAtIndex(index, newValue);
        }

        private string CheckPrefix(string proxyAddress, bool replacePrimary = true)
        {
            if (string.IsNullOrWhiteSpace(proxyAddress))
                throw new ArgumentNullException(nameof(proxyAddress));

            else if (!proxyAddress.StartsWith(Strings.STMP_Prefix, _ignoreCase))
                return Strings.STMP_Prefix + proxyAddress;

            else if (proxyAddress.StartsWith(Strings.SMTP_Prefix_Primary) && replacePrimary && this.ContainsPrimary())
            {
                this.ReducePrimaries();
                return proxyAddress;
            }

            else
                return proxyAddress;
        }


        private static string MakeSecondary(string primaryAlias)
        {
            return Regex.Replace(primaryAlias, Strings.SMTP_Prefix_PrimaryRegex, Strings.STMP_Prefix);
        }
        private void ReducePrimaries()
        {
            int index = -1;
            do
            {
                index = this.InnerList.FindIndex(add => add.StartsWith(Strings.SMTP_Prefix_Primary));
                if (index > -1)
                {
                    this.InnerList[index] = MakeSecondary(this.InnerList[index]);
                }
            }
            while (index > -1);
        }

        //private static void DoListAction(string item, Action<string> action)
        //{
        //    item = CheckPrefix(item);
        //    action(item);
        //}
        //private static T DoListFunction<T>(string item, Func<string, T> function)
        //{
        //    item = CheckPrefix(item);
        //    return function(item);
        //}
    }
}
