using System.Collections.Generic;
using softWrench.sW4.Configuration.Definitions;

namespace softWrench.sW4.Configuration.Util {
    internal class CategoryUtil {

        internal static IEnumerable<KeyValuePair<string, Category>> BuildCategoryEntries(string catKey) {
            return BuildCategoryEntries(catKey, null);
        }

        public static IDictionary<string, Category> BuildCategoryDict(IDictionary<string, PropertyDefinition> toRegister) {
            var result = new Dictionary<string, Category>();
            foreach (var entry in toRegister) {
                var propertyKey = entry.Key;
                var categoryKey = GetCategoryFullKey(propertyKey);
                if (!result.ContainsKey(categoryKey)) {
                    var buildCategoryEntries = BuildCategoryEntries(categoryKey);
                    foreach (var catEntry in buildCategoryEntries) {
                        catEntry.Value.FullKey = catEntry.Key;
                        result.Add(catEntry.Key, catEntry.Value);
                    }
                }
            }
            return result;
        }

        private static IEnumerable<KeyValuePair<string, Category>> BuildCategoryEntries(string catKey, Category childCategory) {
            catKey = Normalize(catKey);
            var result = new List<KeyValuePair<string, Category>>();
            var category = new Category();
            var lastidx = catKey.LastIndexOf("/", System.StringComparison.Ordinal);
            var penultimateidx = catKey.LastIndexOf("/", lastidx - 1, System.StringComparison.Ordinal);
            category.Key = catKey.Substring(penultimateidx + 1, lastidx - penultimateidx - 1);
            result.Add(new KeyValuePair<string, Category>(catKey, category));
            if (childCategory != null) {
                childCategory.ParentCategory = category;
            }
            if (penultimateidx != 0) {
                var parentCategory = catKey.Substring(0, penultimateidx);
                result.AddRange(BuildCategoryEntries(parentCategory, category));
            }
            return result;
        }

        private static string Normalize(string catKey) {
            if (!catKey.EndsWith("/")) {
                catKey = catKey + "/";
            }
            if (!catKey.StartsWith("/")) {
                catKey = "/" + catKey;
            }
            return catKey;
        }

        public static string GetCategoryParentKey(string fullKey) {
            var lastidx = fullKey.LastIndexOf("/", System.StringComparison.Ordinal);
            var penultimateidx = fullKey.LastIndexOf("/", lastidx - 1, System.StringComparison.Ordinal);
            if (penultimateidx != 0) {
                return fullKey.Substring(0, penultimateidx+1);
            }
            return null;
        }

        public static string GetCategoryFullKey(string fullKey) {
            var lastSlash = fullKey.LastIndexOf("/", System.StringComparison.Ordinal);
            if (lastSlash == fullKey.Length - 1) {
                return fullKey;
            }
            return fullKey.Substring(0, lastSlash + 1);
        }

        public static string GetLastKey(string fullKey) {
            var lastidx = fullKey.LastIndexOf("/", System.StringComparison.Ordinal);
            var penultimateidx = fullKey.LastIndexOf("/", lastidx - 1, System.StringComparison.Ordinal);
            return fullKey.Substring(penultimateidx + 1, lastidx - penultimateidx - 1);
        }

        public static string GetPropertyKey(string fullKey) {
            var lastSlash = fullKey.LastIndexOf("/", System.StringComparison.Ordinal);
            return fullKey.Substring(lastSlash + 1, fullKey.Length - lastSlash - 1);
        }
    }
}
