using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Path
{
    public class LdapUriBuilder : PathBuilderBase, IPathBuilder
    {
        private const int _LDAP_PORT = 389;
        private const int _LDAPS_PORT = 636;

        private const string _protocol = "LDAP";
        private int _port = _LDAP_PORT;

        public override int Port => _port;
        public sealed override string Protocol => _protocol;
        public override bool SSL
        {
            get => _LDAPS_PORT == _port;
            set
            {
                _port = value ? _LDAPS_PORT : _LDAP_PORT;
            }
        }
        public LdapUriBuilder(string domainOrDC)
            : base(domainOrDC)
        {
        }
    }
}
