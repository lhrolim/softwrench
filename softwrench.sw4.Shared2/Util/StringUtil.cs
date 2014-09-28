using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace softwrench.sw4.Shared2.Util {
    public static class StringUtil {

        /// <summary>
        /// Gets all occurrences of a sub-string between a given 'start' and 'end' strings
        /// </summary>
        /// <param name="input">Input string to be parsed</param>
        /// <param name="start">Start string delimiter</param>
        /// <param name="end">End string delimiter</param>
        /// <returns>Enumerator with all string occurrences</returns>
        public static IEnumerable<string> GetSubStrings(string input, string start, string end) {

            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
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

            Regex r = new Regex(Regex.Escape(start) + @"(.*?)(?=\W)");
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

        public static string ReplaceFirstOccurrence(this string source, string find, string replace) {
            var place = source.IndexOf(find, System.StringComparison.Ordinal);
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

        public static bool NullOrEmpty(this string source) {
            return String.IsNullOrEmpty(source);
        }

    }
}
