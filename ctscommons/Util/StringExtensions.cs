using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ctscommons.Util {
    public static class StringExtensions {
        public static byte[] GetBytes(this string str) {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string Fmt(this string str, params object[] parameters) {
            return string.Format(str, parameters);
        }

        public static bool EqualsAny(this string str, params string[] strings) {
            return strings.Any(toCompare => str.Equals(toCompare, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool EqualsIc(this string str, string other) {
            return str.Equals(other, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool EqualsAny(this string str, IEnumerable<string> strings) {
            return strings.Any(toCompare => str.Equals(toCompare, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool StartsWithAny(this string str, params string[] strings) {
            return strings.Any(toCompare => str.StartsWith(toCompare, StringComparison.InvariantCultureIgnoreCase));
        }

        public static int GetNumberOfItems(this string str, string toSearch) {
            return new Regex(Regex.Escape(toSearch)).Matches(str).Count;
        }
    }
}
