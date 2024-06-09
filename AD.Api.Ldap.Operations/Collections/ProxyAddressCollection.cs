using MG.Collections;
using System.Collections;

namespace AD.Api.Ldap.Properties
{
    public sealed class ProxyAddressCollection : UniqueList<ProxyAddress>
    {
        public ProxyAddressCollection()
            : base()
        {
        }

        public ProxyAddressCollection(ICollection collection)
            : base(collection.Count)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));

            foreach (string address in collection.Cast<string>())
            {
                this.Add(address);
            }
        }

        public ProxyAddressCollection(IEnumerable<string> proxies)
            : base(GetCount(proxies))
        {
            foreach (string address in proxies.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                this.Add(address);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPrimary"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public int AddNewPrimary(ProxyAddress newPrimary)
        {
            if (newPrimary is null)
                throw new ArgumentNullException(nameof(newPrimary));

            else if (!newPrimary.IsValid)
                throw new InvalidOperationException($"Cannot make an invalid '{nameof(ProxyAddress)}' primary - {newPrimary.GetValue()}");

            else if (!newPrimary.IsPrimary)
                newPrimary.IsPrimary = true;

            int newIndex;
            int currentPrimaryIndex = this.IndexOfPrimary();
            if (currentPrimaryIndex <= -1)
            {
                this.Add(newPrimary);
                newIndex = this.IndexOf(newPrimary);
            }
            else
            {
                this[currentPrimaryIndex].IsPrimary = false;
                this.Add(newPrimary);
                newIndex = this.IndexOf(newPrimary);
            }

            return newIndex;
        }

        public void AddRange(IEnumerable<string> addresses)
        {
            foreach (string address in addresses.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                this.Add(address);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toThis"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public int ChangePrimary(ProxyAddress toThis)
        {
            int index = this.IndexOf(toThis);
            if (index <= -1)
                return this.AddNewPrimary(toThis);

            else if (toThis.IsPrimary)
                throw new InvalidOperationException($"'{toThis.GetValue()}' is already the primary address.");

            int currentPrimaryIndex = this.IndexOfPrimary();
            if (currentPrimaryIndex > -1)
                this[currentPrimaryIndex].IsPrimary = false;

            this[index].IsPrimary = true;

            return index;
        }

        public int ChangePrimary(Func<ProxyAddress, bool> toThisAddress)
        {
            ProxyAddress? address = this.Find(toThisAddress);
            if (!(address is null))
                return this.ChangePrimary(address);

            else
                return -1;
        }

        public void ForEach(Action<ProxyAddress> action)
        {
            for (int i = 0; i < this.Count; i++)
            {
                action(this[i]);
            }
        }

        public bool HasPrimary()
        {
            return this.Count > 0
                   &&
                   this.Exists(x => x.IsPrimary);
        }
        public int IndexOfPrimary()
        {
            return this.FindIndex(x => x.IsPrimary);
        }

        private static int GetCount(IEnumerable<string>? proxies)
        {
            return proxies is null
                ? 0
                : proxies.TryGetNonEnumeratedCount(out int count)
                    ? count
                    : 0;
        }
    }
}
