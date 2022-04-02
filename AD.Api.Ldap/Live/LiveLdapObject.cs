using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using AD.Api.Ldap.Path;

namespace AD.Api.Ldap.Live
{
    public class LiveLdapObject : IDisposable
    {
        private bool _disposed;
        private readonly DirectoryEntry _dirEntry;

        public LiveLdapObject(IPathBuilder builder)
            : this(builder.ToPath())
        {
        }
        public LiveLdapObject(string path)
        {
            _dirEntry = new DirectoryEntry(path);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dirEntry.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
