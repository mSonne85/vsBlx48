using System.Collections.Generic;

namespace vsBlx48.Collections.Generic
{
    /// <summary>
    /// Supports a simple iteration over a generic key/value pairs collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key objects to enumerate.</typeparam>
    /// <typeparam name="TValue">The type of the value objects to enumerate.</typeparam>
    public interface IEnumerator<TKey, TValue> : IEnumerator<Entity<TKey, TValue>>
    {
        /// <summary>
        /// Gets the key in the collection at the current position of the enumerator.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// Gets the value in the collection at the current position of the enumerator.
        /// </summary>
        TValue Value { get; }
    }
}
