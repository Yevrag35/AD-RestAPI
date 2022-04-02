using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using AD.Api.Ldap.Path;

namespace AD.Api.Ldap
{
    public class LiveLdapObject : IDisposable
    {
        private bool _disposed;
        private readonly PathValue _path;
        private readonly DirectoryEntry _dirEntry;

        public PathValue Path => _path;

        public LiveLdapObject(DirectoryEntry entry)
        {
            _dirEntry = entry;
            _path = PathValue.FromDirectoryEntry(entry);
        }

        #region IDISPOSABLE IMPLEMENTATION
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _path.Clear();
                    _dirEntry.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        #endregion
    }
}
