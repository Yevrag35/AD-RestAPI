using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AD.Api.Models.Collections
{
    public class ADValueList<T> : List<T>, IValueCollection<T>
    {
        public bool EnforcesUnique => false;
        public bool SortsAlways => false;
    }
}
