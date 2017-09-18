using System;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Util {

    public class VersionUtil {

        public static bool IsGreaterThan(string currentVersionSt, string allowedVersions) {
            if (currentVersionSt == null) {
                return true;
            }

            if (allowedVersions == null) {
                return true;
            }

            if (currentVersionSt.EqualsIc("ripple")) {
                //to allow testing ripple emulator, declare a !ripple on the version config
                if (allowedVersions.EqualsIc("!ripple")) {
                    return false;
                }
                return true;
            }

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
