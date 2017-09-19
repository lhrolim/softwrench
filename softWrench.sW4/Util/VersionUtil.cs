using System;
using cts.commons.portable.Util;
using log4net;

namespace softWrench.sW4.Util {

    public class VersionUtil {


        private static readonly ILog Log = LogManager.GetLogger(typeof(VersionUtil));

        public static bool IsGreaterThan(string currentVersionSt, string allowedVersions) {
            if (currentVersionSt == null) {
                return true;
            }

            if (string.IsNullOrEmpty(allowedVersions)) {
                return true;
            }

            if (currentVersionSt.EqualsIc("ripple")) {
                //to allow testing ripple emulator, declare a !ripple on the version config
                if (allowedVersions.EqualsIc("!ripple")) {
                    return false;
                }
                return true;
            }

            //pull requests come with # commit number
            var idx = currentVersionSt.IndexOf("#", StringComparison.Ordinal);
            if (idx != -1) {
                currentVersionSt = currentVersionSt.Substring(0, idx);
            }

            if (currentVersionSt.EndsWith("-SNAPSHOT")) {
                currentVersionSt = currentVersionSt.Replace("-SNAPSHOT", "");
            }
         


            Log.DebugFormat("current version {0}", currentVersionSt);

            var currentVersion = new Version(currentVersionSt);
            var versionsSt = allowedVersions.Split(',');
            foreach (var versionst in versionsSt) {
                var version = new Version(versionst);
                if (currentVersion >= version) {
                    return true;
                }
            }

            return false;
        }

    }
}
