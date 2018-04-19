using System;
using System.Collections.Generic;
using System.Linq;

namespace Emerald.Application
{
    public static class ValidationHelper
    {
        public static bool IsNull<T>(T? value, out T result) where T : struct
        {
            result = value ?? default(T);
            return value == null;
        }
        public static bool IsNotNull<T>(T? value, out T result) where T : struct
        {
            return !IsNull(value, out result);
        }
        public static bool IsNullOrEmptyOrWhiteSpace(string str)
        {
            return string.IsNullOrWhiteSpace(str) || str == string.Empty;
        }
        public static bool IsNotNullOrEmptyOrWhiteSpace(string str)
        {
            return !IsNullOrEmptyOrWhiteSpace(str);
        }
        public static bool IsLink(string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        public static bool IsNotLink(string str)
        {
            return !IsLink(str);
        }
        public static bool IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }
        public static bool IsNotNullOrEmpty<T>(IEnumerable<T> collection)
        {
            return !IsNullOrEmpty(collection);
        }
        public static bool IsDefined<T>(T value) where T : struct
        {
            return Enum.IsDefined(typeof(T), value);
        }
        public static bool IsNotDefined<T>(T value) where T : struct
        {
            return !IsDefined(value);
        }
    }
}