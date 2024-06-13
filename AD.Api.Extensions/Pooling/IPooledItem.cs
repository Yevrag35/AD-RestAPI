using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Pooling
{
    /// <summary>
    /// An interface that represents an object that was retrieved from a pool and when disposed, will return 
    /// itself to the pool.
    /// </summary>
    /// <typeparam name="T">The type of object that was retrieved and can be returned.</typeparam>
    /// <typeparam name="TPool">The type of pool the object will be returned to.</typeparam>
    public interface IPooledItem<T> : IDisposable where T : notnull
    {
        /// <summary>
        /// The unique identifier of the lease that was acquired when the object was retrieved from the pool.
        /// </summary>
        Guid LeaseId { get; }

        /// <summary>
        /// The borrowed object that will be available for the duration of the lifetime of this instance.
        /// </summary>
        T Value { get; }
    }
}

