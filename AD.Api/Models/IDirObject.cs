using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace AD.Api.Models
{
    public interface IDirObject
    {
        public string DistinguishedName { get; }
        public DirectoryEntry GetDirectoryEntry(string domainController);
    }
}
