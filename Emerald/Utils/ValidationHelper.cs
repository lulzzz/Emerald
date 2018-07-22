using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Emerald.Utils
{
    public static class ValidationHelper
    {
        private static readonly string[] ImageExtensionArray = { ".jpg", ".jpeg", ".bmp", ".gif", ".png" };

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
        public static bool IsImageLink(string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && ImageExtensionArray.Contains(Path.GetExtension(str));
        }
        public static bool IsNotImageLink(string str)
        {
            return !IsImageLink(str);
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
        public static bool IsEmail(string str)
        {
            if (IsNullOrEmptyOrWhiteSpace(str)) return false;
            if (str.Length > 64) return false;

            var invalid = true;

            string MatchEvaluator(Match match)
            {
                var idnMapping = new IdnMapping();
                var domainName = match.Groups[2].Value;

                try
                {
                    domainName = idnMapping.GetAscii(domainName);
                }
                catch (ArgumentException)
                {
                    invalid = true;
                }

                return match.Groups[1].Value + domainName;
            }

            try
            {
                str = Regex.Replace(str, @"(@)(.+)$", MatchEvaluator, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid) return false;

            try
            {
                const string pattern =
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

                return Regex.IsMatch(str, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static bool IsNotEmail(string str)
        {
            return !IsEmail(str);
        }
    }
}