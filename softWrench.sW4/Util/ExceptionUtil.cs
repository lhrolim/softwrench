using NHibernate.Util;
using System;
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
    }
}
