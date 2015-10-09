using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softwrench.sw4.Shared2.Metadata.Modules;

namespace softWrench.sW4.Configuration.Util {
    public class CategoryDTO : IComparable<CategoryDTO> {
        public string Key { get; set; }

        public ModuleDefinition Module { get; set; }
        public int? UserProfile { get; set; }

        /// <summary>
        /// Only the profiles that have permissions to use the application should be able to set whereclauses
        /// </summary>
        public HashSet<int?> AllowedForProfiles {
            get; set;
        }

        public WhereClauseRegisterCondition Condition { get; set; }

        public string FullKey { get; set; }

        public IDictionary<string,string> _valuesToSave = new Dictionary<string, string>();

        public SortedSet<PropertyDefinition> Definitions = new SortedSet<PropertyDefinition>();

        public CategoryDTO() {

        }

        public CategoryDTO(string fullKey) {
            FullKey = fullKey;
            Key = CategoryUtil.GetLastKey(fullKey);
        }


        public int Compare(CategoryDTO x, CategoryDTO y) {
            return System.String.Compare(x.Key, y.Key, System.StringComparison.Ordinal);
        }

        public int CompareTo(CategoryDTO other) {
            return System.String.Compare(Key, other.Key, System.StringComparison.Ordinal);
        }

        public override string ToString() {
            return string.Format("Key: {0}", Key);
        }

        public SortedSet<CategoryDTO> Children = new SortedSet<CategoryDTO>();

        public IDictionary<string, string> ValuesToSave
        {
            get { return _valuesToSave; }
            set { _valuesToSave = value; }
        }

        [JsonIgnore]
        public CategoryDTO Parent { get; set; }


    }
}
