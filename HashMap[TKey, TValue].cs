//------------------------------------------------------------------------------------------------------------------------------------------------------------------
// File: "HashMap[T1, T2].cs" | Authors: "Sonne170" | Date: 05.12.2024 | Rev. Date: 28.11.2024 | Version: 1.0.0.0
//------------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace vsBlx48.Collections.Generic
{
    /// <summary>
    /// Represents a collection of key/value pairs whose insertion order is guaranteed when iterating over the collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the hashmap.</typeparam>
    /// <typeparam name="TValue">The type of the values in the hashmap.</typeparam>
    [DebuggerDisplay("Tag = {Tag}, Count = {Count}")]
    public class HashMap<TKey, TValue> : IHashMap<TKey, TValue>, IReadOnlyHashMap<TKey, TValue>
    {
        // This class is basically a Dictionary<TKey, TValue> but has three major differences.
        //
        // 1: This class is also a doubly linked circular list, which means that the
        // elements inserted into this table preserve their insertion order.
        //
        // 2: This class relies more on the Entity<T1, T2> structure instead of
        // the KeyValuePair<TKey, TValue> structure. For more information on this, see Entity.cs
        //
        // 3: based on "DE0006: Non-generic collections shouldn't be used"
        // https://github.com/dotnet/platform-compat/blob/master/docs/DE0006.md
        // This class no longer implements the IDictionary and IDictionaryEnumerator interfaces.

        //===========================================================================================================================================================
        // MEMBER VARIABLES
        //===========================================================================================================================================================

        static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761,
            919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };  // microsoft original predefined prime table 

        //---------------------------------------------------------------------------------------------------------

        int first           = 0;        // index of the logically first inserted entry
        int last            = 0;        // index of the logically last inserted entry
        int size            = 0;        // total number of entry including removed entry
        int freeindex       = -1;       // index of the logically last removed entry
        int freecount       = 0;        // total number of entry removed
        int[] buckets       = null;     // array whose index is the result of [hashcode % length]
        Entry[] entries     = null;     // actual array to store the entry

        //---------------------------------------------------------------------------------------------------------

        object tag                          = null;     // optional data associated with the hashmap
        KeyCollection keys                  = null;     // implementation of IEnumerator<TKey>
        ValueCollection values              = null;     // implementation of IEnumerator<TValue>
        IEqualityComparer<TKey> comparer    = null;     // hash code provider and comparer for keys

        //===========================================================================================================================================================
        // CONSTRUCTORS
        //===========================================================================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{TKey,TValue}"/> class that is empty, has the default initial capacity, and uses the default equality comparer.
        /// </summary>
        public HashMap() : this(0, null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{TKey,TValue}"/> class that is empty, has the specified initial capacity, and uses the default equality comparer.
        /// </summary>
        /// <param name="capacity">The initial number of elements the hashmap can contain.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public HashMap(int capacity) : this(capacity, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{TKey,TValue}"/> class that is empty, has the default initial capacity, and uses the specified equality comparer.
        /// </summary>
        /// <param name="comparer">The equality comparer implementation to use when comparing keys, or null to use the default equality comparer.</param>
        public HashMap(IEqualityComparer<TKey> comparer) : this(0, comparer)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{TKey,TValue}"/> class that is empty, has the specified initial capacity, and uses the specified equality comparer.
        /// </summary>
        /// <param name="capacity">The initial number of elements the hashmap can contain.</param>
        /// <param name="comparer">The equality comparer implementation to use when comparing keys, or null to use the default equality comparer.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public HashMap(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
                Throw(Exceptions.ArgumentOutOfRange, nameof(capacity), msgCapacity);

            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            int prime = GetPrime(capacity);
            entries = new Entry[prime]; buckets = new int[prime];

            for (int i = 0; i < prime; i++) buckets[i] = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{TKey,TValue}"/> class that contains elements copied from the specified collection and uses the default equality comparer.
        /// </summary>
        /// <param name="hashmap">The collection whose elements are copied to the new hashmap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public HashMap(IHashMap<TKey, TValue> hashmap) : this(hashmap, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashMap{TKey,TValue}"/> class that contains elements copied from the specified collection and uses the specified equality comparer.
        /// </summary>
        /// <param name="hashmap">The collection whose elements are copied to the new hashmap.</param>
        /// <param name="comparer">The equality comparer implementation to use when comparing keys, or null to use the default equality comparer.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public HashMap(IHashMap<TKey, TValue> hashmap, IEqualityComparer<TKey> comparer)
        {
            if (hashmap == null)
                Throw(Exceptions.ArgumentNull, nameof(hashmap));

            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            InsertRange(hashmap, false); // initializes "entries" and "buckets"
        }

        //===========================================================================================================================================================
        // PUBLIC PROPERTIES
        //===========================================================================================================================================================

        /// <summary>
        /// Gets or sets the optional data associated with the hashmap.
        /// </summary>
        public object Tag
        {
            get => tag;
            set => tag = value;
        }

        /// <summary>
        /// Gets or sets the equality comparer to use when comparing and hashing keys.
        /// </summary>
        public IEqualityComparer<TKey> Comparer
        {
            get => comparer; set
            {
                // yes...the hashmap class allows to change the IEqualityComparer implementation.
                // Now there is no need to create a new instance to provide a different Comparer.

                if (value == null)
                    value = EqualityComparer<TKey>.Default;

                if (value != comparer)
                {
                    if (size - freecount > 0)
                    {
                        int length = buckets.Length; // reset
                        for (int i = length; --i >= 0;) buckets[i] = -1;

                        int hashcode, bucket, current = first; do // insertion order
                        {
                            ref Entry entry = ref entries[current];
                            hashcode = value.GetHashCode(entry.key) & 0x7FFFFFFF;
                            bucket = hashcode % length;
                            entry.prior = buckets[bucket];
                            entry.hashcode = hashcode;
                            buckets[bucket] = current;

                            current = entry.next;
                        }
                        while (current != first);
                    }
                    comparer = value;
                }
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs the hashmap contains.
        /// </summary>
        public int Count
        {
            get => size - freecount;
        }

        /// <summary>
        /// Gets or sets the number of elements the internal data structure can hold without resizing.
        /// <br>If the specified value is not a prime, a suitable prime number is determined.</br>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Capacity
        {
            get => entries.Length; set
            {
                if (value < size) // caution: we have to pay attention to "freeindex" and "freecount"
                    Throw(Exceptions.ArgumentOutOfRange, nameof(value), msgCapacity);

                // Since the default behavior of Dictionary<TKey, TValue> is to always grow twice its
                // current size, I decided to add this property to provide more flexibility if needed.
                // However, keep in mind gaps in the "entries" array are not closed using this property.

                if (value > entries.Length)
                    EnsureCapacity(GetPrime(value));
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public TValue this[TKey key]
        {
            get
            {
                int index = IndexOf(key);

                if (index < 0)
                    Throw(Exceptions.KeyNotFound, null);

                return entries[index].value;
            }
            set
            {
                Internal_Insert(key, value, true);
            }
        }

        /// <summary>
        /// Gets a collection containing the keys of the hashmap.
        /// </summary>
        public KeyCollection Keys
        {
            get
            {
                if (keys == null)
                    keys = new KeyCollection(this);

                return keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values of the hashmap.
        /// </summary>
        public ValueCollection Values
        {
            get
            {
                if (values == null)
                    values = new ValueCollection(this);

                return values;
            }
        }

        //===========================================================================================================================================================
        // PRIVATE METHODS
        //===========================================================================================================================================================

        static int GetPrime(int size)
        {
            if (size < 7199369) // try predefined table first
            {
                for (int i = size < 12143 ? 0 : 36; i < primes.Length; i++)
                    if (primes[i] >= size) return primes[i];
            }

            // outside of predefined table, compute the hard way
            if ((uint)size < int.MaxValue)
            {
                size |= 1; int j; double sqrt;
                for (int i = size; i <= int.MaxValue; i += 2)
                {
                    sqrt = Math.Sqrt(i);

                    for (j = 3; j <= sqrt; j += 2)
                    {
                        if (i % j == 0) break;
                    }
                    if (j > sqrt) return i;
                }
            }
            return int.MaxValue;
        }

        void EnsureCapacity(int newsize)
        {
            Entry[] newentries = new Entry[newsize];
            int[] newbuckets = new int[newsize];
            for (int i = 0; i < newsize; i++) newbuckets[i] = -1;

            if (size - freecount > 0)
            {
                Array.Copy(entries, 0, newentries, 0, size);
                int bucket, current = first; do
                {
                    // we need to update all the elements in "entries" and
                    // "buckets" to match the result [hashcode % newsize]
                    ref Entry entry = ref newentries[current];
                    bucket = entry.hashcode % newsize;
                    entry.prior = newbuckets[bucket];
                    newbuckets[bucket] = current;

                    current = entry.next;
                }
                while (current != first);
            }

            buckets = newbuckets;
            entries = newentries;
        }

        int IndexOf(TKey key)
        {
            if (key == null)
                Throw(Exceptions.ArgumentNull, nameof(key));

            int hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int bucket = hashcode % buckets.Length;
            
            if (buckets[bucket] >= 0)
            {
                for (int i = buckets[bucket]; i >= 0; i = entries[i].prior)
                    if (entries[i].hashcode == hashcode && comparer.Equals(entries[i].key, key)) return i;
            }

            return -1; // not found
        }

        void Internal_Insert(TKey key, TValue value, bool replace)
        {
            if (key == null)
                Throw(Exceptions.ArgumentNull, nameof(key));

            int hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int index, bucket = hashcode % buckets.Length;
            
            if (buckets[bucket] >= 0) { // determine specified key already exists
                for (index = buckets[bucket]; index >= 0; index = entries[index].prior) {
                    if (entries[index].hashcode == hashcode && comparer.Equals(entries[index].key, key))
                    {
                        if (replace) { entries[index].value = value; return; }
                        Throw(Exceptions.Argument, nameof(key), msgDuplicate);
                    }
                }
            }

            if (freecount > 0) {  index = freeindex; freecount--; // close gaps first
                freeindex = entries[index].prior; /*see Remove*/ goto AddEntry; }

            if (size == entries.Length) {
                EnsureCapacity(GetPrime(entries.Length * 2));
                bucket = hashcode % buckets.Length; // size of "buckets" changed
            }
            index = size; size++; AddEntry:;

            entries[first].prev = index;            // link first entry to last entry
            entries[index].next = first;            // link last entry to first entry

            entries[last].next = index;             // link previous entry to current entry
            entries[index].prev = last;             // link current entry to previous entry
            last = index;                           //

            entries[index].hashcode = hashcode;     //
            entries[index].key = key;               // 
            entries[index].value = value;           //

            entries[index].prior = buckets[bucket]; // buckets[bucket] != -1, index of previous entry with the same bucket
            buckets[bucket] = index;                // register/ update to the latest index
        }

        void Internal_InsertRange(Entity<TKey, TValue>[] array, bool? replace)
        {
            // Unfortunately, there is no easy way to distinguish between
            // Entity and KeyValuePair arrays, otherwise there would be
            // only one InsertRange method...

            int count = array.Length, x = size - freecount + count;
            if (x >= entries.Length) EnsureCapacity(GetPrime(x));

            int hashcode, bucket, index; TKey key; for (x = 0; x < count; x++)
            {
                if ((key = array[x].First) == null) continue;

                hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                bucket = hashcode % buckets.Length;

                if (buckets[bucket] >= 0) { // determine specified key already exists
                    for (index = buckets[bucket]; index >= 0; index = entries[index].prior)
                        if (entries[index].hashcode == hashcode && comparer.Equals(entries[index].key, key))
                        {
                            if (replace == null)
                                Throw(Exceptions.Argument, nameof(key), msgDuplicate);

                            if (replace.Value) entries[index].value = array[x].Second; goto MoveNext;
                        }
                }

                if (freecount > 0) { index = freeindex; freecount--; // close gaps first
                    freeindex = entries[index].prior; } else { index = size; size++; }

                ref Entry entry = ref entries[index];
                entries[last].next = index;             // link previous entry to current entry
                entry.prev = last; last = index;        // link current entry to previous entry

                entry.hashcode = hashcode;              //
                entry.key = key;                        //
                entry.value = array[x].Second;          //

                entry.prior = buckets[bucket];          // buckets[bucket] != -1, index of previous entry with the same bucket
                buckets[bucket] = index; MoveNext:;     // register/ update to the latest index
            }
            entries[first].prev = last;     // link first entry to last entry
            entries[last].next = first;     // link last entry to first entry
        }

        void Internal_InsertRange(KeyValuePair<TKey, TValue>[] array, bool? replace)
        {
            // Unfortunately, there is no easy way to distinguish between
            // Entity and KeyValuePair arrays, otherwise there would be
            // only one InsertRange method...

            int count = array.Length, x = size - freecount + count;
            if (x >= entries.Length) EnsureCapacity(GetPrime(x));

            int hashcode, bucket, index; TKey key; for (x = 0; x < count; x++)
            {
                if ((key = array[x].Key) == null) continue;

                hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                bucket = hashcode % buckets.Length;

                if (buckets[bucket] >= 0) { // determine specified key already exists
                    for (index = buckets[bucket]; index >= 0; index = entries[index].prior)
                        if (entries[index].hashcode == hashcode && comparer.Equals(entries[index].key, key))
                        {
                            if (replace == null)
                                Throw(Exceptions.Argument, nameof(key), msgDuplicate);

                            if (replace.Value) entries[index].value = array[x].Value; goto MoveNext;
                        }
                }

                if (freecount > 0) { index = freeindex; freecount--; // close gaps first
                    freeindex = entries[index].prior; } else { index = size; size++; }

                ref Entry entry = ref entries[index];
                entries[last].next = index;             // link previous entry to current entry
                entry.prev = last; last = index;        // link current entry to previous entry

                entry.hashcode = hashcode;              //
                entry.key = key;                        //
                entry.value = array[x].Value;           //

                entry.prior = buckets[bucket];          // buckets[bucket] != -1, index of previous entry with the same bucket
                buckets[bucket] = index; MoveNext:;     // register/ update to the latest index
            }
            entries[first].prev = last;     // link first entry to last entry
            entries[last].next = first;     // link last entry to first entry
        }

        void CopyEntries(Entry[] source, int start, int count, bool rehash)
        {
            // Creates a clean copy of the source array without copying
            // it's gaps from removing elements. Only use this method
            // if "size - freecount == 0"!

            // if source equals entries => TrimExcess, count is a prime
            if (source != entries) count = GetPrime(count);

            // TrimExcess do not work without temporary array!
            Entry[] temp = new Entry[count]; buckets = new int[count];
            for (int i = 0; i < count; i++) buckets[i] = -1;

            int hashcode, bucket;
            int current = start, index = 0, prev = 0; do
            {
                ref Entry entry = ref source[current];  // take a reference is faster
                hashcode = !rehash ? entry.hashcode :   // force new hash codes if source comparer != this comparer
                    comparer.GetHashCode(entry.key) & 0x7FFFFFFF;

                temp[prev].next = index;                // link previous entry to current entry
                temp[index].prev = prev;                // link current entry to previous entry
                temp[index].hashcode = hashcode;        // 
                temp[index].key = entry.key;            // 
                temp[index].value = entry.value;        //

                bucket = hashcode % count;              //
                temp[index].prior = buckets[bucket];    // buckets[bucket] != -1, index of previous entry with the same bucket
                buckets[bucket] = index;                // register/ update to the latest index

                prev = index; index++;
                current = entry.next;
            }
            while (current != start);

            temp[first].prev = prev;     // link first entry to last entry
            temp[prev].next = first;     // link last entry to first entry

            first = 0; last = prev; size = index;
            freeindex = -1; freecount = 0; entries = temp;
        }

        void CopyEntries(Entry[] source, int start, int count, bool rehash, bool? replace)
        {
            // Creates a clean copy of the source array without copying
            // it's gaps from removing elements. Use this method
            // if "size - freecount > 0" & "source != this.entries".

            int x = size - freecount + count;
            if (x >= entries.Length) EnsureCapacity(GetPrime(x));

            int hashcode, bucket, index;
            int current = start; count = buckets.Length; do 
            {
                ref Entry entry = ref source[current];  // take a reference is faster
                hashcode = !rehash ? entry.hashcode :   // force new hash codes if source comparer != this comparer
                    comparer.GetHashCode(entry.key) & 0x7FFFFFFF;
                bucket = hashcode % count;             // compute the buckets[index]

                if (buckets[bucket] >= 0) { // determine specified key already exists
                    for (index = buckets[bucket]; index >= 0; index = entries[index].prior)
                        if (entries[index].hashcode == hashcode && comparer.Equals(entries[index].key, entry.key))
                        {
                            if (replace == null)
                                Throw(Exceptions.Argument, nameof(entry.key), msgDuplicate);

                            if (replace.Value) entries[index].value = entry.value; goto MoveNext;
                        }
                }

                if (freecount > 0) { index = freeindex; freecount--; // close gaps first
                    freeindex = entries[index].prior; } else { index = size; size++; }

                entries[last].next = index;                 // link previous entry to current entry
                entries[index].prev = last;                 // link current entry to previous entry
                last = index;                               //

                entries[index].hashcode = hashcode;         //
                entries[index].key = entry.key;             //
                entries[index].value = entry.value;         //

                entries[index].prior = buckets[bucket];     // buckets[bucket] != -1, index of previous entry with the same bucket
                buckets[bucket] = index;                    // register/ update to the latest index

                MoveNext:; current = entry.next;
            }
            while (current != start);

            entries[first].prev = last;        // link first entry to last entry
            entries[last].next = first;        // link last entry to first entry
        }

        //===========================================================================================================================================================
        // PUBLIC METHODS
        //===========================================================================================================================================================

        /// <summary>
        /// Adds the specified key and value to the hashmap.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Add(TKey key, TValue value)
        {
            Internal_Insert(key, value, false);
        }

        /// <summary>
        /// Attempts to insert the specified key and value into the hashmap.
        /// </summary>
        /// <param name="key">The key of the element to insert.</param>
        /// <param name="value">The value of the element to insert. Can be null for reference types.</param>
        /// <param name="replace">If true, already existing elements are replaced, otherwise preserved.</param>
        /// <returns><see langword="true"/> if the specified key/value pair was successfully inserted; otherwise <see langword="false"/>.</returns>
        public bool Insert(TKey key, TValue value, bool replace)
        {
            // for performance reasons, this method is a copy of Insert(TKey, TValue, bool);

            if (key == null) return false;

            int hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int index, bucket = hashcode % buckets.Length;

            if (buckets[bucket] >= 0) { // determine specified key already exists
                for (index = buckets[bucket]; index >= 0; index = entries[index].prior)
                    if (entries[index].hashcode == hashcode && comparer.Equals(entries[index].key, key)) {
                        if (!replace) return false; entries[index].value = value; return true; } }

            if (freecount > 0) { index = freeindex; freecount--; // close gaps first
                freeindex = entries[index].prior; /*see Remove*/ goto AddEntry; }

            if (size == entries.Length) {
                EnsureCapacity(GetPrime(entries.Length * 2));
                bucket = hashcode % buckets.Length; // size of "buckets" changed
            }
            index = size; size++; AddEntry:;

            entries[first].prev = index;            // link first entry to last entry
            entries[index].next = first;            // link last entry to first entry

            entries[last].next = index;             // link previous entry to current entry
            entries[index].prev = last;             // link current entry to previous entry
            last = index;                           // update "last" entry index

            entries[index].hashcode = hashcode;     //
            entries[index].key = key;               // 
            entries[index].value = value;           //

            entries[index].prior = buckets[bucket]; // buckets[bucket] != -1, index of previous entry with the same bucket
            buckets[bucket] = index;                // register/ update to the latest index

            return true;
        }

        /// <summary>
        /// Adds the elements of the specified enumerable to the hashmap.
        /// This method does not <see langword="throw"/> if a specified key is <see langword="null"/>.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements should be added to the hashmap.</param>
        /// <returns>The number of elements added to the hashmap.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public int AddRange(IEnumerable<Entity<TKey, TValue>> enumerable)
        {
            // AddRange does not throw any exception if a specified key is null or
            // already exists. Instead, null keys are ignored and existing entries
            // are either overwritten or skipped, depending on the "overwrite" value.

            if (enumerable == null)
                Throw(Exceptions.ArgumentNull, nameof(enumerable));

            int oldcount = size - freecount; switch (enumerable)
            {
                case HashMap<TKey, TValue> table:
                {
                    if (table.Count == 0) break;

                    if (size - freecount == 0) { // this instance is empty
                        CopyEntries(table.entries, table.first, table.Count,
                            table.comparer != comparer); break;
                    }
                    CopyEntries(table.entries, table.first, table.Count,
                        table.comparer != comparer, null); break;
                }
                case Entity<TKey, TValue>[] array:
                {
                    if (array.Length > 0)
                        Internal_InsertRange(array, null); break;
                }
                case ICollection<Entity<TKey, TValue>> list:
                {
                    if (list.Count > 0) { // List<T>, Collection<T>...
                        var temp = new Entity<TKey, TValue>[list.Count];
                        list.CopyTo(temp, 0); Internal_InsertRange(temp, null); } break;
                }
                default:
                {
                    // fallback path for all other enumerables
                    if (enumerable.Count() > 0)
                        Internal_InsertRange(enumerable.ToArray(), null); break;
                }
            }
            return size - freecount - oldcount;
        }

        /// <summary>
        /// Adds the elements of the specified enumerable to the hashmap.
        /// This method does not <see langword="throw"/> if a specified key is <see langword="null"/>.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements should be added to the hashmap.</param>
        /// <returns>The number of elements added to the hashmap.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            // AddRange does not throw any exception if a specified key is null or
            // already exists. Instead, null keys are ignored and existing entries
            // are either overwritten or skipped, depending on the "overwrite" value.

            if (enumerable == null)
                Throw(Exceptions.ArgumentNull, nameof(enumerable));

            int oldcount = size - freecount; switch (enumerable)
            {
                case KeyValuePair<TKey, TValue>[] array:
                {
                    if (array.Length > 0)
                        Internal_InsertRange(array, null); break;
                }
                case ICollection<KeyValuePair<TKey, TValue>> list:
                {
                    if (list.Count > 0) { // Dictionary, List<T>, Collection<T>...
                        var temp = new KeyValuePair<TKey, TValue>[list.Count];
                        list.CopyTo(temp, 0); Internal_InsertRange(temp, null); } break;
                }
                default:
                {
                    // fallback path for all other enumerables
                    if (enumerable.Count() > 0)
                        Internal_InsertRange(enumerable.ToArray(), null); break;
                }
            }
            return size - freecount - oldcount;
        }

        /// <summary>
        /// Attempts to insert the elements of the specified enumerable into the hashmap.
        /// This method does not <see langword="throw"/> if a specified key is <see langword="null"/> or already exists.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements should be inserted into the hashmap.</param>
        /// <param name="replace">If true, already existing elements are replaced, otherwise preserved.</param>
        /// <returns>The number of elements inserted into the hashmap.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int InsertRange(IEnumerable<Entity<TKey, TValue>> enumerable, bool replace)
        {
            // AddRange does not throw any exception if a specified key is null or
            // already exists. Instead, null keys are ignored and existing entries
            // are either overwritten or skipped, depending on the "overwrite" value.

            if (enumerable == null)
                Throw(Exceptions.ArgumentNull, nameof(enumerable));

            int oldcount = size - freecount; switch (enumerable)
            {
                case HashMap<TKey, TValue> table:
                {
                    if (table.Count == 0) break;

                    if (size - freecount == 0) { // this instance is empty
                        CopyEntries(table.entries, table.first, table.Count,
                            table.comparer != comparer); break;
                    }
                    CopyEntries(table.entries, table.first, table.Count,
                        table.comparer != comparer, replace); break;
                }
                case Entity<TKey, TValue>[] array:
                {
                    if (array.Length > 0)
                        Internal_InsertRange(array, replace); break;
                }
                case ICollection<Entity<TKey, TValue>> list:
                {
                    if (list.Count > 0) { // List<T>, Collection<T>...
                        var temp = new Entity<TKey, TValue>[list.Count];
                        list.CopyTo(temp, 0); Internal_InsertRange(temp, replace); } break;
                }
                default:
                {
                    // fallback path for all other enumerables
                    if (enumerable.Count() > 0)
                        Internal_InsertRange(enumerable.ToArray(), replace); break;
                }
            }
            return size - freecount - oldcount;
        }

        /// <summary>
        /// Attempts to insert the elements of the specified enumerable into the hashmap.
        /// This method does not <see langword="throw"/> if a specified key is <see langword="null"/> or already exists.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements should be inserted into the hashmap.</param>
        /// <param name="replace">If true, already existing elements are replaced, otherwise preserved.</param>
        /// <returns>The number of elements inserted into the hashmap.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int InsertRange(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, bool replace)
        {
            // AddRange does not throw any exception if a specified key is null or
            // already exists. Instead, null keys are ignored and existing entries
            // are either overwritten or skipped, depending on the "overwrite" value.

            if (enumerable == null)
                Throw(Exceptions.ArgumentNull, nameof(enumerable));

            int oldcount = size - freecount; switch (enumerable)
            {
                case KeyValuePair<TKey, TValue>[] array:
                {
                    if (array.Length > 0)
                        Internal_InsertRange(array, replace); break;
                }
                case ICollection<KeyValuePair<TKey, TValue>> list:
                {
                    if (list.Count > 0) { // Dictionary, List<T>, Collection<T>...
                        var temp = new KeyValuePair<TKey, TValue>[list.Count];
                        list.CopyTo(temp, 0); Internal_InsertRange(temp, replace); } break;
                }
                default:
                {
                    // fallback path for all other enumerables
                    if (enumerable.Count() > 0)
                        Internal_InsertRange(enumerable.ToArray(), replace); break;
                }
            }
            return size - freecount - oldcount;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Removes the element with the specified key from the hashmap.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Remove(TKey key)
        {
            if (key == null)
                Throw(Exceptions.ArgumentNull, nameof(key));

            int hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int bucket = hashcode % buckets.Length, prev = -1;

            // start with the last entry added whose index is the result of [hashcode % length]
            for (int i = buckets[bucket]; i >= 0; prev = i, i = entries[i].prior)
            {
                if (entries[i].hashcode == hashcode && comparer.Equals(entries[i].key, key))
                {
                    // first match, just update buckets
                    if (prev < 0) buckets[bucket] = entries[i].prior;
                    // there are more elements with the same bucket
                    else entries[prev].prior = entries[i].prior;

                    // current.next.prev == current.prev
                    entries[entries[i].next].prev = entries[i].prev;
                    // current.prev.next == current.next
                    entries[entries[i].prev].next = entries[i].next;

                    if (i == first) first = entries[i].next;
                    else if (i == last) last = entries[i].prev;

                    entries[i].hashcode = -1;
                    entries[i].next = -1;
                    entries[i].prev = -1;
                    entries[i].prior = freeindex; // see Insert...
                    entries[i].key = default;
                    entries[i].value = default;

                    freeindex = i; freecount++; return true;
                }
                //prev = i; // same as above
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from the hashmap, and copies its value into the value parameter.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns><see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Remove(TKey key, out TValue value)
        {
            // for performance reasons, this method is a copy of Remove(TKey key)

            if (key == null)
                Throw(Exceptions.ArgumentNull, nameof(key));

            int hashcode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int bucket = hashcode % buckets.Length, prev = -1;

            // start with the last entry added whose index is the result of [hashcode % length]
            for (int i = buckets[bucket]; i >= 0; prev = i, i = entries[i].prior)
            {
                if (entries[i].hashcode == hashcode && comparer.Equals(entries[i].key, key))
                {
                    // first match, just update buckets
                    if (prev < 0) buckets[bucket] = entries[i].prior;
                    // there are more elements with the same bucket
                    else entries[prev].prior = entries[i].prior;

                    value = entries[i].value;

                    // current.next.prev == current.prev
                    entries[entries[i].next].prev = entries[i].prev;
                    // current.prev.next == current.next
                    entries[entries[i].prev].next = entries[i].next;

                    if (i == first) first = entries[i].next;
                    else if (i == last) last = entries[i].prev;

                    entries[i].hashcode = -1;
                    entries[i].next = -1;
                    entries[i].prev = -1;
                    entries[i].prior = freeindex; // see Insert...
                    entries[i].key = default;
                    entries[i].value = default;

                    freeindex = i; freecount++; return true;
                }
                //prev = i; // same as above
            }
            value = default; return false;
        }

        /// <summary>
        /// Removes the elements of the specified enumerable from the hashmap.
        /// This method does not <see langword="throw"/> if a specified key is <see langword="null"/>.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements should be removed from the hashmap.</param>
        /// <returns>The number of elements removed from the hashmap.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int RemoveRange(IEnumerable<TKey> enumerable)
        {
            if (enumerable == null)
                Throw(Exceptions.ArgumentNull, nameof(enumerable));

            TKey[] keys; if (enumerable is TKey[] array) {
                keys = array;
            }
            else if (enumerable is ICollection<TKey> list) {
                keys = new TKey[list.Count]; list.CopyTo(keys, 0);
            }
            else keys = enumerable.ToArray(); // fallback path
            
            //-----------------------------------------------------------------------------------------

            int hashcode, bucket, prev, oldcount = freecount;
            int length = keys.Length; for (int x = 0; x < length; x++)
            {
                if (keys[x] == null) continue;

                hashcode = comparer.GetHashCode(keys[x]) & 0x7FFFFFFF;
                bucket = hashcode % buckets.Length; prev = -1;

                for (int i = buckets[bucket]; i >= 0; prev = i, i = entries[i].prior)
                {
                    if (entries[i].hashcode == hashcode && comparer.Equals(entries[i].key, keys[x]))
                    {
                        // first match, just update buckets
                        if (prev < 0) buckets[bucket] = entries[i].prior;
                        // there are more elements with the same bucket
                        else entries[prev].prior = entries[i].prior;

                        // current.next.prev == current.prev
                        entries[entries[i].next].prev = entries[i].prev;
                        // current.prev.next == current.next
                        entries[entries[i].prev].next = entries[i].next;

                        if (i == first) first = entries[i].next;
                        else if (i == last) last = entries[i].prev;

                        entries[i].hashcode = -1;
                        entries[i].next = -1;
                        entries[i].prev = -1;
                        entries[i].prior = freeindex; // see Insert...
                        entries[i].key = default;
                        entries[i].value = default;

                        freeindex = i; freecount++; break;
                    }
                    //prev = i; // same as above
                }
            }
            return freecount - oldcount;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether the hashmap contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the hashmap.</param>
        /// <returns><see langword="true"/> if the hashmap contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ContainsKey(TKey key)
        {
            return IndexOf(key) >= 0;
        }

        /// <summary>
        /// Determines whether the hashmap contains a specific value.
        /// </summary>
        /// <param name="value">The value to locate in the hashmap. The value can be null for reference types.</param>
        /// <returns><see langword="true"/> if the hashmap contains an element with the specified value; otherwise, <see langword="false"/>.</returns>
        public bool ContainsValue(TValue value)
        {
            if (size - freecount == 0) return false;

            int current = first; if(value == null)
            {
                do {
                    if (entries[current].value == null)
                        return true; current = entries[current].next;
                }
                while (current != first); return false;
            }

            do {
                IEqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                if (c.Equals(entries[current].value, value))
                    return true; current = entries[current].next;
            }
            while (current != first); return false;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the logically first element added to the hashmap.
        /// </summary>
        /// <returns>The logically first element added to the hashmap.</returns>
        public Entity<TKey, TValue> GetFirst()
        {
            if (size - freecount > 0)
            {
                return new Entity<TKey, TValue>(entries[first].key, entries[first].value);
            }
            return default;
        }

        /// <summary>
        /// Gets the logically last element added to the hashmap.
        /// </summary>
        /// <returns>The logically last element added to the hashmap.</returns>
        public Entity<TKey, TValue> GetLast()
        {
            if (size - freecount > 0)
            {
                return new Entity<TKey, TValue>(entries[last].key, entries[last].value);
            }
            return default;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns><see langword="true"/> if the hashmap contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key != null)
            {
                int index = IndexOf(key);

                if (index >= 0)
                {
                    value = entries[index].value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Performs a <see langword="do while"/> iteration and executes the specified action on each element of the hashmap.
        /// </summary>
        /// <param name="action">The action delegate to execute on each element of the hashmap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ForEach(Action<TKey, TValue> action)
        {
            if (action == null)
                Throw(Exceptions.ArgumentNull, nameof(action));

            if (size - freecount > 0)
            {
                int current = first; do
                {
                    ref Entry entry = ref entries[current];
                    action(entry.key, entry.value);
                    current = entry.next;
                }
                while (current != first);
            }
        }

        /// <summary>
        /// Converts the elements in the current hashmap to another type, and returns an array containing the converted elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the target array.</typeparam>
        /// <param name="converter">The func delegate that converts each element from one type to another type.</param>
        /// <returns>An array of the target type containing the converted elements from the current hashmap.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T[] Parse<T>(Func<TKey, TValue, T> converter)
        {
            if (converter == null)
                Throw(Exceptions.ArgumentNull, nameof(converter));

            if (size - freecount == 0)
                return new T[0];

            T[] array = new T[size - freecount];
            int current = first, index = 0; do
            {
                ref Entry entry = ref entries[current];
                array[index] = converter(entry.key, entry.value);
                index++; current = entry.next;
            }
            while (current != first); return array;
        }

        /// <summary>
        /// Copies the entire hashmap to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from hashmap. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(Entity<TKey, TValue>[] array, int index)
        {
            if (array == null)
                Throw(Exceptions.ArgumentNull, nameof(array));

            if ((uint)index > array.Length)
                Throw(Exceptions.ArgumentOutOfRange, nameof(index), msgArrayBounds);

            if (size - freecount > array.Length)
                Throw(Exceptions.Argument, nameof(array), msgArrayLength);

            if (size - freecount > 0)
            {
                int current = first; do
                {
                    ref Entry entry = ref entries[current];
                    array[index].First = entry.key;
                    array[index].Second = entry.value;
                    index++; current = entry.next;
                }
                while (current != first);
            }
        }

        /// <summary>
        /// Copies the entire hashmap to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from hashmap. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
                Throw(Exceptions.ArgumentNull, nameof(array));

            if ((uint)index > array.Length)
                Throw(Exceptions.ArgumentOutOfRange, nameof(index), msgArrayBounds);

            if (size - freecount > array.Length)
                Throw(Exceptions.Argument, nameof(array), msgArrayLength);

            if (size - freecount > 0)
            {
                int current = first; do
                {
                    ref Entry entry = ref entries[current];
                    array[index] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
                    index++; current = entry.next;
                }
                while (current != first);
            }
        }

        /// <summary>
        /// Copies the elements of the hashmap to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the hashmap.</returns>
        public Entity<TKey, TValue>[] ToArray()
        {
            if (size - freecount > 0)
            {
                Entity<TKey, TValue>[] array = new Entity<TKey, TValue>[size - freecount];

                int current = first, index = 0; do
                {
                    ref Entry entry = ref entries[current];
                    array[index].First = entry.key;
                    array[index].Second = entry.value;
                    index++; current = entry.next;
                }
                while (current != first); return array;
            }
            return new Entity<TKey, TValue>[0];
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Removes all keys and values from the hashmap.
        /// </summary>
        public void Clear()
        {
            freecount = 0; freeindex = -1;
            first = 0; last = 0; size = 0;

            Array.Clear(entries, 0, size);
            for (int i = buckets.Length; --i >= 0;) buckets[i] = -1;
        }

        /// <summary>
        /// Sets the capacity of this hashmap to what it would be if it had been originally initialized with all its entries.
        /// </summary>
        public void TrimExcess()
        {
            if (size - freecount == 0) return;
            int prime = GetPrime(size - freecount);
            if (prime >= entries.Length) return;

            CopyEntries(entries, first, prime, false);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the hashmap.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        //===========================================================================================================================================================
        // INTERFACE IMPLEMENTATIONS
        //===========================================================================================================================================================

        ICollection<TKey> IHashMap<TKey, TValue>.Keys
        {
            get
            {
                if (keys == null)
                    keys = new KeyCollection(this);

                return keys;
            }
        }

        ICollection<TValue> IHashMap<TKey, TValue>.Values
        {
            get
            {
                if (values == null)
                    values = new ValueCollection(this);

                return values;
            }
        }

        IEnumerable<TKey> IReadOnlyHashMap<TKey, TValue>.Keys
        {
            get
            {
                if (keys == null)
                    keys = new KeyCollection(this);

                return keys;
            }
        }

        IEnumerable<TValue> IReadOnlyHashMap<TKey, TValue>.Values
        {
            get
            {
                if (values == null)
                    values = new ValueCollection(this);

                return values;
            }
        }

        bool ICollection<Entity<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void ICollection<Entity<TKey, TValue>>.Add(Entity<TKey, TValue> item)
        {
            Internal_Insert(item.First, item.Second, false);
        }

        bool ICollection<Entity<TKey, TValue>>.Contains(Entity<TKey, TValue> item)
        {
            int index = IndexOf(item.First);

            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(
                item.Second, entries[index].value)) return true;

            return false;

            //return index >= 0 && EqualityComparer<TValue>.Default.Equals(
            //    item.Item2, entries[index].value);
        }

        void ICollection<Entity<TKey, TValue>>.CopyTo(Entity<TKey, TValue>[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        bool ICollection<Entity<TKey, TValue>>.Remove(Entity<TKey, TValue> item)
        {
            int index = IndexOf(item.First); if (index >= 0)
            {
                EqualityComparer<TValue> cp = EqualityComparer<TValue>.Default;

                if (cp.Equals(item.Second, entries[index].value))
                    return Remove(item.First);
            }
            return false;
        }

        IEnumerator<Entity<TKey, TValue>> IEnumerable<Entity<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        //===========================================================================================================================================================
        // NESTED TYPES
        //===========================================================================================================================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public int hashcode;    // lower 31 bits of hash code, -1 if unused
            public int prior;       // index of previous entry with the same bucked, -1 if last
            public int next;        // index of next entry, "first" if last
            public int prev;        // index of previous entry, "last" if first
            public TKey key;        // key of entry
            public TValue value;    // value of entry

            public override string ToString()
            {
                // improve debug view
                return string.Format("{0}, {1}, {2}", key, value, prior);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<TKey, TValue>
        {
            int index;
            int count;

            readonly int size;
            readonly HashMap<TKey, TValue> parent;

            Entity<TKey, TValue> current;

            internal Enumerator(HashMap<TKey, TValue> parent)
            {
                this.parent = parent;
                size = parent.Count;
                index = parent.first;
                count = 0; current = default;
            }

            public TKey Key
            {
                get
                {
                    //if (count == 0 || (count == size + 1))
                    //    Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                    return current.First;
                }
            }

            public TValue Value
            {
                get
                {
                    //if (count == 0 || (count == size + 1))
                    //    Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                    return current.Second;
                }
            }

            public Entity<TKey, TValue> Current
            {
                get
                {
                    //if (count == 0 || (count == size + 1))
                    //    Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                    return current;
                }
            }

            public bool MoveNext()
            {
                // unsigned comparison since count = size + 1
                // could be negative if size is Int32.MaxValue
                if ((uint)count < (uint)size)
                {
                    ref Entry entry = ref parent.entries[index];
                    current = new Entity<TKey, TValue>(entry.key, entry.value);
                    index = entry.next; count++; return true;
                }

                count = size + 1; index = -1;
                current = default; return false;
            }

            public void Dispose()
            {
                // foreach automatically calls dispose

                //-----------------------------------------------------------------------

                // note: IEnumerator does not implement IDisposable

                //using (IEnumerator<T> enumerator = table.GetEnumerator())
                //{
                //    while (enumerator.MoveNext())
                //    {
                //        // object IEnumerator.Current
                //        Debug.WriteLine(enumerator.Current);
                //    }
                //} automatically calls dispose

                //-----------------------------------------------------------------------

                // while (enumerator.MoveNext())
                // {
                //     ...
                // }
                // enumerator.Dispose(); // has to be called manually
            }

            object IEnumerator.Current
            {
                get
                {
                    if (count == 0 || (count == size + 1))
                        Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                    return current;
                }
            }

            void IEnumerator.Reset()
            {
                index = parent.first;
                count = 0; current = default;
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>
        {
            readonly HashMap<TKey, TValue> parent;

            public KeyCollection(HashMap<TKey, TValue> parent)
            {
                this.parent = parent;
            }

            public int Count
            {
                get
                {
                    return parent.Count;
                }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                Throw(Exceptions.NotSupported, null, msgReadOnly);
            }

            void ICollection<TKey>.Clear()
            {
                Throw(Exceptions.NotSupported, null, msgReadOnly);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                Throw(Exceptions.NotSupported, null, msgReadOnly);
                return false;
            }

            public bool Contains(TKey item)
            {
                return parent.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                if (array == null)
                    Throw(Exceptions.ArgumentNull, nameof(array));

                if ((uint)arrayIndex >= array.Length)
                    Throw(Exceptions.ArgumentOutOfRange, nameof(arrayIndex), msgArrayBounds);

                if (array.Length - arrayIndex < parent.Count)
                    Throw(Exceptions.Argument, nameof(array), msgArrayLength);

                if (parent.Count > 0)
                {
                    int first = parent.first;
                    Entry[] entries = parent.entries;

                    int current = first; do
                    {
                        ref Entry entry = ref entries[current];
                        array[arrayIndex++] = entry.key;
                        current = entry.next;
                    }
                    while (current != first);
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(parent);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(parent);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator(parent);
            }

            public struct Enumerator : IEnumerator<TKey>
            {
                int index;
                int count;

                readonly int size;
                readonly HashMap<TKey, TValue> parent;

                TKey current;

                internal Enumerator(HashMap<TKey, TValue> parent)
                {
                    this.parent = parent;
                    size = parent.Count;
                    index = parent.first;
                    count = 0; current = default;
                }

                public TKey Current
                {
                    get
                    {
                        //if (count == 0 || (count == size + 1))
                        //    Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                        return current;
                    }
                }

                public bool MoveNext()
                {
                    if ((uint)count < (uint)size)
                    {
                        ref Entry entry = ref parent.entries[index];
                        current = entry.key;
                        index = entry.next; count++; return true;
                    }

                    count = size + 1; index = -1;
                    current = default; return false;
                }

                public void Dispose()
                {

                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (count == 0 || (count == size + 1))
                            Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                        return current;
                    }
                }

                void IEnumerator.Reset()
                {
                    index = parent.first;
                    count = 0; current = default;
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection : ICollection<TValue>
        {
            readonly HashMap<TKey, TValue> parent;

            public ValueCollection(HashMap<TKey, TValue> parent)
            {
                this.parent = parent;
            }

            public int Count
            {
                get
                {
                    return parent.Count;
                }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                Throw(Exceptions.NotSupported, null, msgReadOnly);
            }

            void ICollection<TValue>.Clear()
            {
                Throw(Exceptions.NotSupported, null, msgReadOnly);
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                Throw(Exceptions.NotSupported, null, msgReadOnly);
                return false;
            }

            public bool Contains(TValue item)
            {
                return parent.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array == null)
                    Throw(Exceptions.ArgumentNull, nameof(array));

                if ((uint)arrayIndex >= array.Length)
                    Throw(Exceptions.ArgumentOutOfRange, nameof(arrayIndex), msgArrayBounds);

                if (array.Length - arrayIndex < parent.Count)
                    Throw(Exceptions.Argument, nameof(array), msgArrayLength);

                if (parent.Count > 0)
                {
                    int first = parent.first;
                    Entry[] entries = parent.entries;

                    int current = first; do
                    {
                        ref Entry entry = ref entries[current];
                        array[arrayIndex++] = entry.value;
                        current = entry.next;
                    }
                    while (current != first);
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(parent);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(parent);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator(parent);
            }

            public struct Enumerator : IEnumerator<TValue>
            {
                int index;
                int count;

                readonly int size;
                readonly HashMap<TKey, TValue> parent;

                TValue current;

                internal Enumerator(HashMap<TKey, TValue> parent)
                {
                    this.parent = parent;
                    size = parent.Count;
                    index = parent.first;
                    count = 0; current = default;
                }

                public TValue Current
                {
                    get
                    {
                        //if (count == 0 || (count == size + 1))
                        //    Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                        return current;
                    }
                }

                public bool MoveNext()
                {
                    if ((uint)count < (uint)size)
                    {
                        ref Entry entry = ref parent.entries[index];
                        current = entry.value;
                        index = entry.next; count++; return true;
                    }

                    count = size + 1; index = -1;
                    current = default; return false;
                }

                public void Dispose()
                {

                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (count == 0 || (count == size + 1))
                            Throw(Exceptions.InvalidOperation, null, msgEnumeration);

                        return current;
                    }
                }

                void IEnumerator.Reset()
                {
                    index = parent.first;
                    count = 0; current = default;
                }
            }
        }

        //===========================================================================================================================================================
        // EXCEPTION HELPER
        //===========================================================================================================================================================

        // This may not be the best solution to get rid of the overhead, but for
        // now this should be better than the old way. If this library continues
        // to grow, this will most likely be replaced by a ThrowHelper class.
        // This also helps to keep additional references small.

        enum Exceptions
        {
            Argument,
            ArgumentNull,
            ArgumentOutOfRange,
            InvalidOperation,
            KeyNotFound,
            NotSupported,
        }

        static readonly string msgDuplicate = "An item with the same key has already been added.";
        static readonly string msgCapacity = "The specified value was less than the current number of elements the collection contains.";
        static readonly string msgArrayBounds = "The specified value was either less than or greater than the array's bounds in the first dimension.";
        static readonly string msgArrayLength = "The destination array was not long enough. Check destination index and length, and the array's lower bounds.";
        static readonly string msgEnumeration = "Enumeration has either not started or has already finished";
        static readonly string msgReadOnly = "Mutating a collection derived from a hashmap is not allowed.";

        static void Throw(Exceptions exceptions, string name, string message = null)
        {
            switch(exceptions)
            {
                case Exceptions.ArgumentNull:
                    throw new ArgumentNullException(name);

                case Exceptions.ArgumentOutOfRange:
                    throw new ArgumentOutOfRangeException(name, message);

                case Exceptions.Argument:
                    throw new ArgumentException(message, name);

                case Exceptions.KeyNotFound:
                    throw new KeyNotFoundException();

                case Exceptions.InvalidOperation:
                    throw new InvalidOperationException(message);

                case Exceptions.NotSupported:
                    throw new NotSupportedException(message);
            }
        }
    }
}