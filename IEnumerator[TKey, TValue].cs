//------------------------------------------------------------------------------------------------------------------------------------------------------------------
// File: "IEnumerator[TKey, TValue].cs" | Authors: "Sonne170" | Date: 28.11.2024 | Rev. Date: 28.11.2024 | Version: 1.0.0.0
//------------------------------------------------------------------------------------------------------------------------------------------------------------------

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
        // The approach of this interface is to replace both IEnumerator<KeyValuePair<TKey, TValue>>
        // and IDictionaryEnumerator to provide a simplified interface for iterating over key/ value
        // pair collections. Based on "DE0006: Non-generic collections shouldn't be used"
        // https://github.com/dotnet/platform-compat/blob/master/docs/DE0006.md
        // the IDictionaryEnumerator interface will no longer be used. Instead the
        // IEnumerator<TKey, TValue> interface adds the strongly typed Key and Value properties.
        // 
        // For more information on why this interface does not use KeyValuePair<TKey, TValue>, see Entity.cs

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