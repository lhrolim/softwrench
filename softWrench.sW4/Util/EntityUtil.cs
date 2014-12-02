using System.Collections.Generic;
using Iesi.Collections.Generic;

namespace softWrench.sW4.Util {

    class EntityUtil {
        public static bool IsRelationshipNameEquals(string firstName, string secondName) {
            return GetRelationshipName(firstName) == GetRelationshipName(secondName);
        }

        public static string GetRelationshipName(string attribute) {
            var idx = attribute.IndexOf(".", System.StringComparison.Ordinal);
            if (idx == -1) {
                return attribute.EndsWith("_") ? attribute.Trim('\'') : attribute.Trim('\'') + "_"; ;
            }
            string relationship = attribute.Substring(0, idx);
            return relationship.EndsWith("_") ? relationship.Trim('\'') : relationship.Trim('\'') + "_";
        }

        public static System.Collections.Generic.ISet<string> GetRelationshipParts(string toSearch) {
            var relationshipParts = new HashSet<string>();
            if (!toSearch.Contains("_")) {
                return relationshipParts;
            }
            string resultName = toSearch;
            do {
                var idx = resultName.IndexOf("_", System.StringComparison.Ordinal);
                if (idx == -1) {
                    break;
                }
                relationshipParts.Add(resultName.Substring(0, idx));
                resultName = resultName.Substring(idx + 1);
            } while (resultName.Contains("_"));
            return relationshipParts;
        }

        public static string GetRelationshipName(string attribute, out string resultAttributeName) {
            var idx = attribute.IndexOf(".", System.StringComparison.Ordinal);
            if (idx == -1) {
                resultAttributeName = null;
                return attribute.EndsWith("_") ? attribute.Trim('\'') : attribute.Trim('\'') + "_"; ;
            }
            string relationship = attribute.Substring(0, idx);
            var lastUnderscoreIdx = relationship.LastIndexOf("_", System.StringComparison.Ordinal);
            var firstUnderscoreIdx = relationship.IndexOf("_", System.StringComparison.Ordinal);
            if (firstUnderscoreIdx != lastUnderscoreIdx && lastUnderscoreIdx != -1) {
                //this means that we have two or more levels
                resultAttributeName = attribute.Substring(firstUnderscoreIdx + 1);
                return relationship.Substring(0, firstUnderscoreIdx + 1).Trim('\'');

            }
            resultAttributeName = attribute.Substring(idx + 1);
            return relationship.EndsWith("_") ? relationship.Trim('\'') : relationship.Trim('\'') + "_";
        }


        public static string GetApplicationName(string relationship) {
            if (!relationship.EndsWith("_")) {
                return relationship;
            }
            return relationship.Substring(0, relationship.Length - 1);
        }


        public static string GetAttributeName(string name) {
            var idx = name.IndexOf(".", System.StringComparison.Ordinal);
            return name.Substring(idx + 1);
        }
    }
}
