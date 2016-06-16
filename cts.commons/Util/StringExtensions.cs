using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace cts.commons.portable.Util {
    public static class StringExtensions {
        public static byte[] GetBytes(this string str) {
            if (str == null) {
                return null;
            }
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes) {
            if (bytes == null) {
                return null;
            }
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string Fmt(this string str, params object[] parameters) {
            return string.Format(str, parameters);
        }

        public static bool EqualsAny(this string str, params string[] strings) {
            return strings.Any(toCompare => str.Equals(toCompare, StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool EqualsIc(this string str, string other) {
            return str.Equals(other, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool EqualsAny(this string str, IEnumerable<string> strings) {
            return strings.Any(toCompare => str.Equals(toCompare, StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool StartsWithAny(this string str, params string[] strings) {
            return strings.Any(toCompare => str.StartsWith(toCompare, StringComparison.CurrentCultureIgnoreCase));
        }

        public static int GetNumberOfItems(this string str, string toSearch) {
            return new Regex(Regex.Escape(toSearch)).Matches(str).Count;
        }

        public static string ReplaceFirstOccurrence(this string source, string find, string replace) {
            var place = source.IndexOf(find, System.StringComparison.Ordinal);
            if (place == -1) {
                return source;
            }
            return source.Remove(place, find.Length).Insert(place, replace);
        }
        public static bool Contains(this string source, string find, StringComparison comparison) {
            return source.IndexOf(find, comparison) >= 0;
        }

        public static bool ContainsIgnoreCase(this string source, string find) {
            return source.Contains(find, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsAny(this string source, string[] find, StringComparison comparison) {
            return find.Any(f => source.Contains(f, comparison));
        }

        public static bool ContainsAnyIgnoreCase(this string source, string[] find) {
            return source.ContainsAny(find, StringComparison.OrdinalIgnoreCase);
        }

        public static T? ToEnum<T>(this string value, T? defaultValue = null) where T : struct {
            if (string.IsNullOrEmpty(value)) {
                return defaultValue;
            }
            T result;
            return Enum.TryParse(value, true, out result) ? result : defaultValue;
        }

    }
}
