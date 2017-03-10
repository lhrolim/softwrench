using System.Text.RegularExpressions;

namespace softwrench.sW4.Shared2.Metadata.Applications {
    public enum ClientPlatform {
        Web,
        Mobile,
        Both
    }

    public static class ClientPlatformExtensions {

        public static bool Match(this ClientPlatform platform, ClientPlatform toCheck) {
            if (platform == ClientPlatform.Both) {
                return true;
            }
            return platform.Equals(toCheck);

        }
    }
}
