using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Path
{
    public abstract class PathBuilderBase
    {
        protected const char FORWARD_SLASH = (char)47;
        private const string CON_FORMAT = "{0}://{1}:{2}/{3}";
        //private protected readonly UriBuilder _builder;
        private string? _dn;
        private string _host;

        public string DistinguishedName
        {
            get => !string.IsNullOrWhiteSpace(_dn) ? _dn : string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _dn = string.Empty;

                else
                    _dn = value.TrimStart(FORWARD_SLASH);
            }
        }

        public string Domain => _host;
        public abstract string Protocol { get; }
        public abstract int Port { get; }
        public abstract bool SSL { get; set; }

        public PathBuilderBase(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentNullException(nameof(host));

            _host = host;
        }

        public string ToPath()
        {
            return string.Format(CultureInfo.CurrentCulture, CON_FORMAT, this.Protocol, this.Domain, this.Port, this.DistinguishedName);
        }
    }
}
