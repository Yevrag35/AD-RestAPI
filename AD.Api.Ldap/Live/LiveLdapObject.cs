using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using AD.Api.Extensions;
using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Path;

namespace AD.Api.Ldap
{
    public class LiveLdapObject : IDisposable
    {
        private bool _disposed;
        private readonly PathValue _path;
        private readonly DirectoryEntry _dirEntry;

        public PathValue Path => _path;
        public DirectoryEntry DirEntry => _dirEntry;

        [LdapProperty("objectClass")]
        public string ObjectClass { get; set; }

        public LiveLdapObject(DirectoryEntry entry)
            : this(entry, PathValue.FromDirectoryEntry(entry))
        {
        }
        internal LiveLdapObject(DirectoryEntry entry, PathValue path)
        {
            _dirEntry = entry;
            _path = path;
            this.ObjectClass = TryGetObjectClass(_dirEntry, out string? objectClass)
                ? objectClass
                : "RootDSE";
        }

        private static bool TryGetObjectClass(DirectoryEntry? entry, [NotNullWhen(true)] out string? objectClass)
        {
            objectClass = null;
            return entry is not null && entry
                .Properties
                    .TryGetAsSingle(nameof(objectClass), out objectClass, col => col.LastOrDefault());
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
                    //_path.Clear();
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
