namespace softwrench.sW4.Shared2.Util {

    public class EntityUtil {
        public static bool IsRelationshipNameEquals(string firstName, string secondName) {
            return GetRelationshipName(firstName).Equals(GetRelationshipName(secondName),System.StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetRelationshipName(string attribute) {
            var idx = attribute.IndexOf(".", System.StringComparison.Ordinal);
            if (idx == -1) {
                return attribute.EndsWith("_") ? attribute : attribute + "_"; ;
            }
            string relationship = attribute.Substring(0, idx);
            return relationship.EndsWith("_") ? relationship : relationship + "_";
        }

        public static string GetRelationshipName(string attribute, out string resultAttributeName) {
            var idx = attribute.IndexOf(".", System.StringComparison.Ordinal);
            if (idx == -1) {
                resultAttributeName = null;
                return attribute.EndsWith("_") ? attribute : attribute + "_"; ;
            }
            string relationship = attribute.Substring(0, idx);
            resultAttributeName = attribute.Substring(idx + 1);
            return relationship.EndsWith("_") ? relationship : relationship + "_";
        }


        public static string GetApplicationName(string relationship) {
            if (!relationship.EndsWith("_")) {
                return relationship;
            }
            return relationship.Substring(0, relationship.Length - 1);
        }

     




    }
}
