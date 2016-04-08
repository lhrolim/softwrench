using NHibernate.Util;
using System;
using System.Diagnostics;
using System.IO;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Util {
    public class ExceptionUtil {
        public static System.Exception DigRootException(System.Exception e) {
            var exceptions = new IdentitySet();
            return DoDig(e, exceptions);
        }

        private static Exception DoDig(Exception e, IdentitySet exceptionsRead) {
            if (e.InnerException != null && !exceptionsRead.Contains(e.InnerException)) {
                exceptionsRead.Add(e.InnerException);
                return DoDig(e.InnerException, exceptionsRead);
            }
            return e;
        }

        public static InvalidOperationException InvalidOperation(String msg, params object[] args) {
            throw new InvalidOperationException(String.Format(msg, args));
        }

        //TODO: modify type of exception
        public static InvalidOperationException MetadataException(String msg, params object[] args) {
            throw new InvalidOperationException(String.Format(msg, args));
        }

        public static string[] StackTraceLines([NotNull]Exception e) {
            return e.StackTrace.Split('\n', '\r', '\t');
        }

        public static string LastStackTraceLine([NotNull]Exception e) {
            var lines = StackTraceLines(e);
            return lines[lines.Length - 1];
        }

        /// <summary>
        /// Returns the first stack trace line that has mention of solution file.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string FirstProjectStackTraceLine([NotNull] Exception e) {
            var st = new StackTrace(e, true);
            var frames = st.GetFrames();
            if (frames == null || !frames.Any()) {
                return FirstStackTraceLine(e);
            }
            string line = null;
            foreach (var frame in frames) {
                var file = frame.GetFileName();
                // skip lines where the file is this class's file
                if (file == null || file.Contains(typeof(ExceptionUtil).FullName)) {
                    continue;
                }
                // test for specific package names (as path fragments)
                if (file.Contains("cts" + Path.DirectorySeparatorChar) || file.Contains(Path.DirectorySeparatorChar + "cts") ||
                    file.Contains("softwrench" + Path.DirectorySeparatorChar) || file.Contains(Path.DirectorySeparatorChar + "softwrench") ||
                    file.ContainsIgnoreCase("sw4" + Path.DirectorySeparatorChar) || file.ContainsIgnoreCase(Path.DirectorySeparatorChar + "sw4")) {
                    // composing the line from scratch because frame.ToString() doesn't come with method's name 
                    // and sometimes comes prefixed with a bit of unwanted garbage
                    line = string.Format("{0} at {1}: {2}: {3}", frame.GetFileName(), frame.GetMethod(), frame.GetFileLineNumber(), frame.GetFileColumnNumber());
                    break;
                }
            }
            return line ?? FirstStackTraceLine(e);
        }

        public static string FirstStackTraceLine([NotNull] Exception e) {
            if (e.StackTrace == null) {
                return "";
            }
            var lines = StackTraceLines(e);
            return lines[0];
        }
    }
}
