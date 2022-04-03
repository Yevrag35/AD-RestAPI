using System;
using System.Collections.Generic;

namespace AD.Api.Models
{
    public interface IValueCollection<T> : IList<T>
    {
        bool EnforcesUnique { get; }
        bool SortsAlways { get; }

        void AddRange(IEnumerable<T> items);
    }
}
