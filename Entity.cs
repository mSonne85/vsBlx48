//------------------------------------------------------------------------------------------------------------------------------------------------------------------
// File: "Entity.cs" | Authors: "Sonne170" | Date: 10.11.2024 | Rev. Date: 10.12.2024 | Version: 1.0.0.0
//------------------------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace vsBlx48.Collections.Generic
{
    // Why don't use KeyValuePair<TKey, TValue>...
    //
    // Inheritance: Object -> ValueType -> KeyValuePair<TKey, TValue>
    // 
    // ...at least over time, Microsoft should have implemented the IComparable,
    // IComparable<T> and IEquatable<T> interfaces to improve performance when
    // comparing two KeyValuePair<TKey, TValue> objects. If the underlying type
    // derived from ValueType is a structure, field reflection is used by
    // ValueType.Equals and ValueType.GetHashCode. This is significantly slower
    // than the counterpart of Entity<TKey, TValue>. Especially the overwritten
    // equals method benefits from this implementation.
    //
    // ...and why don't use Tuple<T1, T2, ...>
    //
    // the reason for this is relatively simple, in most cases structures
    // offer a better performance than classes. Keep in mind that structures
    // are designed to group different data into one data set, while classes
    // are designed to handle data for further processing, editing, converting
    // mutating and so on.


    /// <summary>
    /// Represents a non-readonly 2-entity, or pair.
    /// </summary>
    /// <typeparam name="T1">The type of the entity's first component.</typeparam>
    /// <typeparam name="T2">The type of the entity's second component.</typeparam>
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Entity<T1, T2> : IComparable, IComparable<Entity<T1, T2>>, IEquatable<Entity<T1, T2>> /*where T1 : IComparable where T2 : IComparable*/
    {
        // fields
        // -----------------------------------------------------------------------------------------------

        T1 first;
        T2 second;

        // properties
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value of the first component.
        /// </summary>
        public T1 First { get => first; set => first = value; } // for release build, property as fast as field

        /// <summary>
        /// Gets or sets the value of the second component.
        /// </summary>
        public T2 Second { get => second; set => second = value; } // for release build, property as fast as field

        // constructors
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{T1, T2}"/> structure.
        /// </summary>
        /// <param name="first">The value of the first component.</param>
        /// <param name="second">The value of the second component.</param>
        public Entity(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }

        // comparability
        // -----------------------------------------------------------------------------------------------

        public int CompareTo(object obj)
        {
            if (obj is Entity<T1, T2> other)
            {
                return CompareTo(other);
            }
            return 1;
        }

        public int CompareTo(Entity<T1, T2> other)
        {
            int result = Comparer<T1>.Default.Compare(first, other.first);
            if (result != 0) return result;

            return Comparer<T2>.Default.Compare(second, other.second);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <param name="component">The component to compare with the component of this instance.
        /// <br>[1] = <see cref="First"/></br><br>[2] = <see cref="Second"/></br></param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value Meaning Less than zero This instance precedes other in the sort order.
        /// Zero This instance occurs in the same position in the sort order as other.
        /// Greater than zero	This instance follows other in the sort order.</returns>
        public int CompareTo(Entity<T1, T2> other, int component)
        {
            switch (component)
            {
                case 1: return Comparer<T1>.Default.Compare(first, other.first);
                case 2: return Comparer<T2>.Default.Compare(second, other.second);

                default: return CompareTo(other); // component out of range...
            }
        }

        // equality
        // -----------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            // override this method otherwise we call ValueType.Equals() which uses reflection!
            if (obj is Entity<T1, T2> other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(Entity<T1, T2> other)
        {
            // override this method otherwise we call ValueType.Equals which uses reflection!

            //if ((object)item1 == (object)other.item1
            //    && (object)item2 == (object)other.item2) return true;

            //if (item1 == null || other.item1 == null
            //    || item2 == null || other.item2 == null) return false;

            //return item1.Equals(other.item1) && item2.Equals(other.item2);

            return EqualityComparer<T1>.Default.Equals(first, other.first)
                && EqualityComparer<T2>.Default.Equals(second, other.second);
        }

        /// <summary>
        /// Indicates whether the component of the current object is equal to the component of another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="component">The component to compare with the component of this object.
        /// <br>[1] = <see cref="First"/></br><br>[2] = <see cref="Second"/></br></param>
        /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Entity<T1, T2> other, int component)
        {
            switch (component)
            {
                case 1: return EqualityComparer<T1>.Default.Equals(first, other.first);
                case 2: return EqualityComparer<T2>.Default.Equals(second, other.second);

                default: return Equals(other); // component out of range...
            }
        }

        public override int GetHashCode()
        {
            // from System.Tuple.CombineHashCodes(int h1, int h2)
            int h1 = first.GetHashCode();
            int h2 = second.GetHashCode();
            return ((h1 << 5) + h1) ^ h2;

            // uses ValueType.GetHashCode() which is significantly slower
            //return base.GetHashCode();
        }

        public static bool operator == (Entity<T1, T2> left, Entity<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Entity<T1, T2> left, Entity<T1, T2> right)
        {
            return !left.Equals(right);
        }

        // type name
        // -----------------------------------------------------------------------------------------------

        public override string ToString()
        {
            // uses StringBuilder
            return string.Format("[{0}, {1}]", first, second);
        }
    }

    /// <summary>
    /// Represents a non-readonly 3-entity, or triple.
    /// </summary>
    /// <typeparam name="T1">The type of the entity's first component.</typeparam>
    /// <typeparam name="T2">The type of the entity's second component.</typeparam>
    /// <typeparam name="T3">The type of the entity's third component.</typeparam>
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Entity<T1, T2, T3> : IComparable, IComparable<Entity<T1, T2, T3>>, IEquatable<Entity<T1, T2, T3>>
    {
        // fields
        // -----------------------------------------------------------------------------------------------

        T1 first;
        T2 second;
        T3 third;

        // properties
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value of the first component.
        /// </summary>
        public T1 First { get => first; set => first = value; }

        /// <summary>
        /// Gets or sets the value of the second component.
        /// </summary>
        public T2 Second { get => second; set => second = value; }

        /// <summary>
        /// Gets or sets the value of the third component.
        /// </summary>
        public T3 Third { get => third; set => third = value; }

        // constructors
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{T1, T2, T3}"/> structure.
        /// </summary>
        /// <param name="first">The value of the first component.</param>
        /// <param name="second">The value of the second component.</param>
        /// <param name="third">The value of the third component.</param>
        public Entity(T1 first, T2 second, T3 third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

        // comparability
        // -----------------------------------------------------------------------------------------------

        public int CompareTo(object obj)
        {
            if (obj is Entity<T1, T2, T3> other)
            {
                return CompareTo(other);
            }
            return 1;
        }

        public int CompareTo(Entity<T1, T2, T3> other)
        {
            int result = Comparer<T1>.Default.Compare(first, other.first);
            if (result != 0) return result;

            result = Comparer<T2>.Default.Compare(second, other.second);
            if (result != 0) return result;

            return Comparer<T3>.Default.Compare(third, other.third);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <param name="component">The component to compare with the component of this instance.
        /// <br>[1] = <see cref="First"/></br><br>[2] = <see cref="Second"/></br><br>[3] = <see cref="Third"/></br></param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value Meaning Less than zero This instance precedes other in the sort order.
        /// Zero This instance occurs in the same position in the sort order as other.
        /// Greater than zero	This instance follows other in the sort order.</returns>
        public int CompareTo(Entity<T1, T2, T3> other, int component)
        {
            switch (component)
            {
                case 1: return Comparer<T1>.Default.Compare(first, other.first);
                case 2: return Comparer<T2>.Default.Compare(second, other.second);
                case 3: return Comparer<T3>.Default.Compare(third, other.third);

                default: return CompareTo(other);
            }
        }

        // equality
        // -----------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if (obj is Entity<T1, T2, T3> other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(Entity<T1, T2, T3> other)
        {
            //if ((object)item1 == (object)other.item1 && (object)item2 == (object)other.item2
            //    && (object)item3 == (object)other.item3) return true;

            //if (item1 == null || other.item1 == null || item2 == null || other.item2 == null
            //    || item3 == null || other.item3 == null) return false;

            //return item1.Equals(other.item1) && item2.Equals(other.item2)
            //    && item3.Equals(other.item3);

            return EqualityComparer<T1>.Default.Equals(first, other.first)
                && EqualityComparer<T2>.Default.Equals(second, other.second)
                && EqualityComparer<T3>.Default.Equals(third, other.third);
        }

        /// <summary>
        /// Indicates whether the component of the current object is equal to the component of another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="component">The component to compare with the component of this object.
        /// <br>[1] = <see cref="First"/></br><br>[2] = <see cref="Second"/></br><br>[3] = <see cref="Third"/></br></param>
        /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Entity<T1, T2, T3> other, int component)
        {
            switch (component)
            {
                case 1: return EqualityComparer<T1>.Default.Equals(first, other.first);
                case 2: return EqualityComparer<T2>.Default.Equals(second, other.second);
                case 3: return EqualityComparer<T3>.Default.Equals(third, other.third);

                default: return Equals(other);
            }
        }

        public override int GetHashCode()
        {
            int h1 = first.GetHashCode();
            int h2 = second.GetHashCode();

            h1 = ((h1 << 5) + h1) ^ h2;
            h2 = third.GetHashCode();

            return ((h1 << 5) + h1) ^ h2;
        }

        public static bool operator == (Entity<T1, T2, T3> left, Entity<T1, T2, T3> right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Entity<T1, T2, T3> left, Entity<T1, T2, T3> right)
        {
            return !left.Equals(right);
        }

        // type name
        // -----------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", first, second, third);
        }
    }

    /// <summary>
    /// Represents a non-readonly 4-entity, or quadruple.
    /// </summary>
    /// <typeparam name="T1">The type of the entity's first component.</typeparam>
    /// <typeparam name="T2">The type of the entity's second component.</typeparam>
    /// <typeparam name="T3">The type of the entity's third component.</typeparam>
    /// <typeparam name="T3">The type of the entity's fourth component.</typeparam>
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Entity<T1, T2, T3, T4> : IComparable, IComparable<Entity<T1, T2, T3, T4>>, IEquatable<Entity<T1, T2, T3, T4>>
    {
        // fields
        // -----------------------------------------------------------------------------------------------

        T1 first;
        T2 second;
        T3 third;
        T4 fourth;

        // properties
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value of the first component.
        /// </summary>
        public T1 First { get => first; set => first = value; }

        /// <summary>
        /// Gets or sets the value of the second component.
        /// </summary>
        public T2 Second { get => second; set => second = value; }

        /// <summary>
        /// Gets or sets the value of the third component.
        /// </summary>
        public T3 Third { get => third; set => third = value; }

        /// <summary>
        /// Gets or sets the value of the fourth component.
        /// </summary>
        public T4 Item4 { get => fourth; set => fourth = value; }

        // constructors
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{T1, T2, T3, T4}"/> structure.
        /// </summary>
        /// <param name="first">The value of the first component.</param>
        /// <param name="second">The value of the second component.</param>
        /// <param name="third">The value of the third component.</param>
        /// <param name="fourth">The value of the fourth component.</param>
        public Entity(T1 first, T2 second, T3 third, T4 fourth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
        }

        // comparability
        // -----------------------------------------------------------------------------------------------

        public int CompareTo(object obj)
        {
            if (obj is Entity<T1, T2, T3, T4> other)
            {
                return CompareTo(other);
            }
            return 1;
        }

        public int CompareTo(Entity<T1, T2, T3, T4> other)
        {
            int result = Comparer<T1>.Default.Compare(first, other.first);
            if (result != 0) return result;

            result = Comparer<T2>.Default.Compare(second, other.second);
            if (result != 0) return result;

            result = Comparer<T3>.Default.Compare(third, other.third);
            if (result != 0) return result;

            return Comparer<T4>.Default.Compare(fourth, other.fourth);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <param name="component">The component to compare with the component of this instance.
        /// <br>[1] = <see cref="Item1"/></br><br>[2] = <see cref="Item2"/></br><br>[3] = <see cref="Item3"/></br><br>[4] = <see cref="Item4"/></br></param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value Meaning Less than zero This instance precedes other in the sort order.
        /// Zero This instance occurs in the same position in the sort order as other.
        /// Greater than zero	This instance follows other in the sort order.</returns>
        public int CompareTo(Entity<T1, T2, T3, T4> other, int component)
        {
            switch (component)
            {
                case 1: return Comparer<T1>.Default.Compare(first, other.first);
                case 2: return Comparer<T2>.Default.Compare(second, other.second);
                case 3: return Comparer<T3>.Default.Compare(third, other.third);
                case 4: return Comparer<T4>.Default.Compare(fourth, other.fourth);

                default: return CompareTo(other);
            }
        }

        // equality
        // -----------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if (obj is Entity<T1, T2, T3, T4> other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(Entity<T1, T2, T3, T4> other)
        {
            //if ((object)item1 == (object)other.item1 && (object)item2 == (object)other.item2
            //    && (object)item3 == (object)other.item3 && (object)item4 == (object)other.item4) return true;

            //if (item1 == null || other.item1 == null || item2 == null || other.item2 == null
            //    || item3 == null || other.item3 == null || item4 == null || other.item4 == null) return false;

            //return item1.Equals(other.item1) && item2.Equals(other.item2)
            //    && item3.Equals(other.item3) && item4.Equals(other.item4);

            return EqualityComparer<T1>.Default.Equals(first, other.first)
                && EqualityComparer<T2>.Default.Equals(second, other.second)
                && EqualityComparer<T3>.Default.Equals(third, other.third)
                && EqualityComparer<T4>.Default.Equals(fourth, other.fourth);
        }

        /// <summary>
        /// Indicates whether the component of the current object is equal to the component of another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="component">The component to compare with the component of this object.
        /// <br>[1] = <see cref="Item1"/></br><br>[2] = <see cref="Item2"/></br><br>[3] = <see cref="Item3"/></br><br>[4] = <see cref="Item4"/></br></param>
        /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Entity<T1, T2, T3, T4> other, int component)
        {
            switch (component)
            {
                case 1: return EqualityComparer<T1>.Default.Equals(first, other.first);
                case 2: return EqualityComparer<T2>.Default.Equals(second, other.second);
                case 3: return EqualityComparer<T3>.Default.Equals(third, other.third);
                case 4: return EqualityComparer<T4>.Default.Equals(fourth, other.fourth);

                default: return Equals(other);
            }
        }

        public override int GetHashCode()
        {
            int h1 = first.GetHashCode();
            int h2 = second.GetHashCode();

            h1 = ((h1 << 5) + h1) ^ h2; // item1 + item2
            h2 = third.GetHashCode();

            h1 = ((h1 << 5) + h1) ^ h2; // (item12) + item3
            h2 = fourth.GetHashCode();

            return ((h1 << 5) + h1) ^ h2; // ((item12)3) + item4
        }

        public static bool operator == (Entity<T1, T2, T3, T4> left, Entity<T1, T2, T3, T4> right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Entity<T1, T2, T3, T4> left, Entity<T1, T2, T3, T4> right)
        {
            return !left.Equals(right);
        }

        // type name
        // -----------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}]", first, second, third, fourth);
        }
    }

    /// <summary>
    /// Represents a non-readonly 5-entity, or quintuple.
    /// </summary>
    /// <typeparam name="T1">The type of the entity's first component.</typeparam>
    /// <typeparam name="T2">The type of the entity's second component.</typeparam>
    /// <typeparam name="T3">The type of the entity's third component.</typeparam>
    /// <typeparam name="T3">The type of the entity's fourth component.</typeparam>
    /// <typeparam name="T3">The type of the entity's fifth component.</typeparam>
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Entity<T1, T2, T3, T4, T5> : IComparable, IComparable<Entity<T1, T2, T3, T4, T5>>, IEquatable<Entity<T1, T2, T3, T4, T5>>
    {
        // fields
        // -----------------------------------------------------------------------------------------------

        T1 first;
        T2 second;
        T3 third;
        T4 fourth;
        T5 fifth;

        // properties
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value of the first component.
        /// </summary>
        public T1 First { get => first; set => first = value; }

        /// <summary>
        /// Gets or sets the value of the second component.
        /// </summary>
        public T2 Second { get => second; set => second = value; }

        /// <summary>
        /// Gets or sets the value of the third component.
        /// </summary>
        public T3 Third { get => third; set => third = value; }

        /// <summary>
        /// Gets or sets the value of the fourth component.
        /// </summary>
        public T4 Fourth { get => fourth; set => fourth = value; }

        /// <summary>
        /// Gets or sets the value of the fifth component.
        /// </summary>
        public T5 Fifth { get => fifth; set => fifth = value; }

        // constructors
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{T1, T2, T3, T4, T5}"/> structure.
        /// </summary>
        /// <param name="first">The value of the first component.</param>
        /// <param name="second">The value of the second component.</param>
        /// <param name="third">The value of the third component.</param>
        /// <param name="fourth">The value of the fourth component.</param>
        /// <param name="fifth">The value of the fifth component.</param>
        public Entity(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
            this.fifth = fifth;
        }

        // comparability
        // -----------------------------------------------------------------------------------------------

        public int CompareTo(object obj)
        {
            if (obj is Entity<T1, T2, T3, T4, T5> other)
            {
                return CompareTo(other);
            }
            return 1;
        }

        public int CompareTo(Entity<T1, T2, T3, T4, T5> other)
        {
            int result = Comparer<T1>.Default.Compare(first, other.first);
            if (result != 0) return result;

            result = Comparer<T2>.Default.Compare(second, other.second);
            if (result != 0) return result;

            result = Comparer<T3>.Default.Compare(third, other.third);
            if (result != 0) return result;

            result = Comparer<T4>.Default.Compare(fourth, other.fourth);
            if (result != 0) return result;

            return Comparer<T5>.Default.Compare(fifth, other.fifth);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <param name="component">The component to compare with the component of this instance.
        /// <br>[1] = <see cref="First"/></br><br>[2] = <see cref="Second"/></br><br>[3] = <see cref="Third"/></br><br>[4] = <see cref="Fourth"/></br><br>[5] = <see cref="Fifth"/></br></param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value Meaning Less than zero This instance precedes other in the sort order.
        /// Zero This instance occurs in the same position in the sort order as other.
        /// Greater than zero	This instance follows other in the sort order.</returns>
        public int CompareTo(Entity<T1, T2, T3, T4, T5> other, int component)
        {
            switch (component)
            {
                case 1: return Comparer<T1>.Default.Compare(first, other.first);
                case 2: return Comparer<T2>.Default.Compare(second, other.second);
                case 3: return Comparer<T3>.Default.Compare(third, other.third);
                case 4: return Comparer<T4>.Default.Compare(fourth, other.fourth);
                case 5: return Comparer<T5>.Default.Compare(fifth, other.fifth);

                default: return CompareTo(other);
            }
        }

        // equality
        // -----------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if (obj is Entity<T1, T2, T3, T4, T5> other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(Entity<T1, T2, T3, T4, T5> other)
        {
            return EqualityComparer<T1>.Default.Equals(first, other.first)
                && EqualityComparer<T2>.Default.Equals(second, other.second)
                && EqualityComparer<T3>.Default.Equals(third, other.third)
                && EqualityComparer<T4>.Default.Equals(fourth, other.fourth)
                && EqualityComparer<T5>.Default.Equals(fifth, other.fifth);
        }

        /// <summary>
        /// Indicates whether the component of the current object is equal to the component of another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="component">The component to compare with the component of this object.
        /// <br>[1] = <see cref="First"/></br><br>[2] = <see cref="Second"/></br><br>[3] = <see cref="Third"/></br><br>[4] = <see cref="Fourth"/></br><br>[5] = <see cref="Fifth"/></br></param>
        /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Entity<T1, T2, T3, T4, T5> other, int component)
        {
            switch (component)
            {
                case 1: return EqualityComparer<T1>.Default.Equals(first, other.first);
                case 2: return EqualityComparer<T2>.Default.Equals(second, other.second);
                case 3: return EqualityComparer<T3>.Default.Equals(third, other.third);
                case 4: return EqualityComparer<T4>.Default.Equals(fourth, other.fourth);
                case 5: return EqualityComparer<T5>.Default.Equals(fifth, other.fifth);

                default: return Equals(other);
            }
        }

        public override int GetHashCode()
        {
            int h1 = first.GetHashCode();
            int h2 = second.GetHashCode();

            h1 = ((h1 << 5) + h1) ^ h2; // item1 + item2
            h2 = third.GetHashCode();

            h1 = ((h1 << 5) + h1) ^ h2; // (item12) + item3
            h2 = fourth.GetHashCode();

            h1 = ((h1 << 5) + h1) ^ h2; // ((item12)3) + item4
            h2 = fifth.GetHashCode();

            return ((h1 << 5) + h1) ^ h2; // (((item12)3)4) + item5
        }

        public static bool operator == (Entity<T1, T2, T3, T4, T5> left, Entity<T1, T2, T3, T4, T5> right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Entity<T1, T2, T3, T4, T5> left, Entity<T1, T2, T3, T4, T5> right)
        {
            return !left.Equals(right);
        }

        // type name
        // -----------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}, {4}]", first, second, third, fourth, fifth);
        }
    }
}