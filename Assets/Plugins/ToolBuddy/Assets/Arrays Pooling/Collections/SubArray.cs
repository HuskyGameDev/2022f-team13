// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using ToolBuddy.Pooling.Pools;

namespace ToolBuddy.Pooling.Collections
{
    /// <summary>
    /// A struct that helps you use a part of an array.
    /// </summary>
    /// <remarks>Can be reused if you free it by calling <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/></remarks>
    /// <typeparam name="T"></typeparam>
#if CURVY_SANITY_CHECKS
    public struct SubArray<T>
    {

        private T[] array;

        /// <summary>
        /// The array where data is stored in
        /// </summary>
        public T[] Array
        {
            get
            {
                if (IsDisposed)
                    throw new InvalidOperationException("Trying to dispose a disposed SubArray");
                return array;
            }
            set => array = value;
        }

        public bool IsDisposed;
#else
public readonly struct SubArray<T>
    {
        /// <summary>
        /// The array where data is stored in
        /// </summary>
        public readonly T[] Array;
#endif
        /// <summary>
        /// The number of elements to be used in that array, counted from the start of the array
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Creates an instance that will use all the elements of the given array
        /// </summary>
        public SubArray(T[] array)
        {
#if CURVY_SANITY_CHECKS
            IsDisposed = false;
            this.array =
#else
            Array = 
#endif
                array != null ? array : throw new ArgumentNullException(nameof(array));
            Count = array.Length;
        }

        /// <summary>
        /// Creates an instance that will use the first "count" elements of the given array
        /// </summary>
        public SubArray(T[] array, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

#if CURVY_SANITY_CHECKS
            IsDisposed = false;
            this.array =
#else
            Array =
#endif
                array;

            Count = count;
        }

        /// <summary>
        /// Returns a new array that which length is <see cref="Count"/> and contains the elements from <see cref="Array"/>
        /// </summary>
        public T[] CopyToArray(ArrayPool<T> arrayPool)
        {
            T[] result = arrayPool.AllocateExactSize(Count, false).Array;
            System.Array.Copy(Array, 0, result, 0, Count);
            return result;
        }

        public override int GetHashCode()
        {
            return Array != null ? Array.GetHashCode() ^ Count : 0;
        }

        public override bool Equals(object obj)
        {
            return obj is SubArray<T> subArray && Equals(subArray);
        }

        public bool Equals(SubArray<T> obj)
        {
            return obj.Array == Array && obj.Count == Count;
        }

        public static bool operator ==(SubArray<T> a, SubArray<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SubArray<T> a, SubArray<T> b)
        {
            return !(a == b);
        }
    }
}