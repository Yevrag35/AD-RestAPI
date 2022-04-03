using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Path
{
    public class GlobalCatalogUriBuilder : PathBuilderBase, IPathBuilder
    {
        private const int _GC_PORT = 3268;
        private const int _GC_SSL_PORT = 3269;

        private const string _protocol = "GC";
        private int _port = _GC_PORT;

        public override int Port => _port;
        public sealed override string Protocol => _protocol;
        public override bool SSL
        {
            get => _GC_SSL_PORT == _port;
            set
            {
                _port = value ? _GC_SSL_PORT : _GC_PORT;
            }
        }

        public GlobalCatalogUriBuilder(string domainOrDC)
            : base(domainOrDC)
        {
        }
    }
}
