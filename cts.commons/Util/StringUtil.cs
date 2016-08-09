using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cts.commons.portable.Util {
    public static class StringUtil {

        /// <summary>
        /// Gets all occurrences of a sub-string between a given 'start' and 'end' strings
        /// </summary>
        /// <param name="input">Input string to be parsed</param>
        /// <param name="start">Start string delimiter</param>
        /// <param name="end">End string delimiter</param>
        /// <returns>Enumerator with all string occurrences</returns>
        public static IEnumerable<string> GetSubStrings(string input, string start, string end) {

            var r = new System.Text.RegularExpressions.Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches) {
                yield return match.Groups[0].Value;
            }
        }

        /// <summary>
        /// Gets all occurrences of a sub-string starting with a given 'start' strings
        /// The end delimiter will be any non-word character
        /// </summary>
        /// <param name="input">Input string to be parsed</param>
        /// <param name="start">Start string delimiter</param>
        /// <returns>Enumerator with all string occurrences</returns>
        public static IEnumerable<string> GetSubStrings(string input, string start) {

            var r = new System.Text.RegularExpressions.Regex(Regex.Escape(start) + @"(.*?)(?=\W)");
            MatchCollection matches = r.Matches(input + " "); // put whitespace in the end to match end delimiter
            foreach (Match match in matches) {
                yield return match.Groups[0].Value;
            }
        }

        public static object FirstLetterToUpper(string s) {
            if (String.IsNullOrEmpty(s)) {
                return s;
            }
            if (s.Length == 1) {
                return s.ToUpper();
            }
            return s.Remove(1).ToUpper() + s.Substring(1);
        }

        public static string ReplaceLastOccurrence(this string source, string find, string replace) {
            var place = source.LastIndexOf(find, System.StringComparison.Ordinal);
            if (place == -1) {
                return source;
            }
            return source.Remove(place, find.Length).Insert(place, replace);
        }

        public static int GetNthIndex(this string source, char charToSearch, int indexToSearch) {
            int count = 0;
            for (int i = 0; i < source.Length; i++) {
                if (source[i] == charToSearch) {
                    count++;
                    if (count == indexToSearch) {
                        return i;
                    }
                }
            }
            return -1;
        }


        public static string SubStringBeforeFirst(this string source, char charToSearch, int initialIndex=0) {
            var idx = source.IndexOf(charToSearch);
            if (idx == -1) {
                return source.Substring(initialIndex);
            }
            return source.Substring(initialIndex, idx-1);
        }


        public static int CountNumberOfOccurrences(this string source, char charToSearch) {
            int count = 0;
            for (int i = 0; i < source.Length; i++) {
                if (source[i] == charToSearch) {
                    count++;
                }
            }
            return count;
        }

        public static bool NullOrEmpty(this string source) {
            return String.IsNullOrEmpty(source);
        }

        public static byte[] GetBytes(string str) {
            var bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

    }
}
