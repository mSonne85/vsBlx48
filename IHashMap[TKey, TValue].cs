//------------------------------------------------------------------------------------------------------------------------------------------------------------------
// File: "IHashMap[TKey, TValue].cs" | Authors: "Sonne170" | Date: 28.11.2024 | Rev. Date: 28.11.2024 | Version: 1.0.0.0
//------------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace vsBlx48.Collections.Generic
{
    /// <summary>
    /// Represents a generic collection of key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of values in the collection.</typeparam>
    public interface IHashMap<TKey, TValue> : ICollection<Entity<TKey, TValue>>
    {
        // This interface is basically identical to the IDictionary<TKey, TValue> interface,
        // but uses the Entity<TKey, TValue> structure instead of KeyValuePair<TKey, TValue>.
        //
        // For more information on why this interface does not use KeyValuePair<TKey, TValue>, see Entity.cs

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        TValue this[TKey key]
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the equality comparer to use when comparing and hashing keys.
        /// </summary>
        IEqualityComparer<TKey> Comparer
        {
            get;
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IHashMap{TKey,TValue}"/>.
        /// </summary>
        ICollection<TKey> Keys
        {
            get;
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the values of the <see cref="IHashMap{TKey,TValue}"/>.
        /// </summary>
        ICollection<TValue> Values
        {
            get;
        }

        /// <summary>
        /// Determines whether the <see cref="IHashMap{TKey,TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="IHashMap{TKey,TValue}"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="IHashMap{TKey,TValue}"/> contains an element with the key; otherwise, <see langword="false"/>.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="IHashMap{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Removes the element with the specified key from the <see cref="IHashMap{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if key was not found in the original <see cref="IHashMap{TKey,TValue}"/>.</returns>
        bool Remove(TKey key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the object that implements <see cref="IHashMap{TKey,TValue}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Performs the specified action on each element of the <see cref="IHashMap{TKey,TValue}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{TKey, TValue}"/> delegate to perform on each element.</param>
        void ForEach(Action<TKey, TValue> action); // alternative approach on IEnumerator
    }
}