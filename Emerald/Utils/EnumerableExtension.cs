using System;
using System.Collections.Generic;
using System.Linq;

namespace Emerald.Utils
{
    public static class EnumerableExtension
    {
        public static bool IsUnique<T>(this IEnumerable<T> collection)
        {
            var array = collection.ToArray();
            return array.Distinct().Count() == array.Length;
        }
        public static bool IsNotUnique<T>(this IEnumerable<T> collection)
        {
            return !IsUnique(collection);
        }
        public static bool NotAll<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return !collection.All(predicate);
        }
    }
}