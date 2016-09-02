using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Metadata.Applications.Relationships.Associations {
    public class ExtraProjectionProviderHelper {
        public static ISet<string> BuildExtraProjectionFields(string extraProjectionFields) {
            var extra = new HashSet<string>();
            if (string.IsNullOrEmpty(extraProjectionFields)) {
                return extra;
            }
            var collection = extraProjectionFields.Split(',');
            foreach (var s in collection) {
                extra.Add(s.Trim());
            }
            return extra;
        }
    }
}
