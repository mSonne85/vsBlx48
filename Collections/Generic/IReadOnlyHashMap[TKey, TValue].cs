using System;
using System.Collections.Generic;

namespace vsBlx48.Collections.Generic
{
    /// <summary>
    /// Represents a strongly-typed, read-only collection of elements.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of values in the collection.</typeparam>
    public interface IReadOnlyHashMap<TKey, TValue> : IReadOnlyCollection<Entity<TKey, TValue>>
    {
        /// <summary>
        /// Gets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The element with the specified key.</returns>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the keys of the <see cref="IReadOnlyHashMap{TKey,TValue}"/>.
        /// </summary>
        IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing the values of the <see cref="IReadOnlyHashMap{TKey,TValue}"/>.
        /// </summary>
        IEnumerable<TValue> Values { get; }

        /// <summary>
        /// Determines whether the <see cref="IReadOnlyHashMap{TKey,TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="IReadOnlyHashMap{TKey,TValue}"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="IReadOnlyHashMap{TKey,TValue}"/> contains an element with the key; otherwise, <see langword="false"/>.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the object that implements <see cref="IReadOnlyHashMap{TKey,TValue}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Performs the specified action on each element of the <see cref="IReadOnlyHashMap{TKey,TValue}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{TKey, TValue}"/> delegate to perform on each element.</param>
        void ForEach(Action<TKey, TValue> action);
    }
}
