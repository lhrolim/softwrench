using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softWrench.sW4.Configuration.Services.Api {

    /// <summary>
    /// This is a dto class to allow registering a whereclause while keeping all relevant information on a single class (mostly are Condition properties, but Module and Profile would be stored directly on the PropertyValue class)
    /// </summary>
    public class WhereClauseRegisterCondition : WhereClauseCondition {

        public string Module {
            get; set;
        }
        public string UserProfile {
            get; set;
        }

        public int? ProfileId {
            get; set;
        }

        public string GenerateAlias() {
            if (Alias != null) {
                return Alias;
            }

            //            if (AppContext?.MetadataId != null) {
            //                //generating an alias based on the metadataid
            //                return AppContext.MetadataId;
            //            }
            IDictionary<string, object> sortedDict = new SortedDictionary<string, object>();
            sortedDict.Add("profileid", ProfileId);
            sortedDict.Add("userprofile", UserProfile);
            if (OfflineOnly) {
                sortedDict.Add("offline", true);
            }
            if (AppContext != null) {
                sortedDict.Add("schema", AppContext.Schema);
                sortedDict.Add("metadataid", AppContext.MetadataId);
            }
            var sb = new StringBuilder();
            foreach (var key in sortedDict.Keys.Where(k => sortedDict[k] != null)) {
                sb.Append(key + ":" + sortedDict[key].ToString().ToLower()).Append(";");
            }
            Alias = sb.ToString();
            return Alias;


        }


        public Condition RealCondition {
            get {
                if (Alias == null) {
                    //check documentation of Alias --> no alias means the framework can´t locate the existing condition to perform an update
                    Alias = GenerateAlias();
                }
                if (AppContext == null && !OfflineOnly) {
                    return (Condition)ReflectionUtil.Clone(new Condition(), this);
                }
                var whereClauseCondition = (WhereClauseCondition)ReflectionUtil.Clone(new WhereClauseCondition(), this);
                if (Global) {
                    //if global, then this could be used by any application
                    whereClauseCondition.FullKey = null;
                }
                return whereClauseCondition;
            }
        }

        public static WhereClauseRegisterCondition FromDataOrNull(int? globalSelectedCondition, string metadataId, int? profileId, bool? offline, string schema = null) {
            if (globalSelectedCondition != null) {
                return new WhereClauseRegisterCondition {
                    Id = globalSelectedCondition,
                    Global = true,
                    ProfileId = profileId
                };
            }


            if (!string.IsNullOrEmpty(metadataId) || profileId.HasValue || offline.HasValue) {
                return new WhereClauseRegisterCondition {
                    AppContext = new ApplicationLookupContext {
                        MetadataId = metadataId,
                        Schema = schema
                    },
                    OfflineOnly = offline ?? false,
                    ProfileId = profileId
                };
            }
            return null;
        }




        public static WhereClauseRegisterCondition ForSchema(string schema) {
            return new WhereClauseRegisterCondition().AppendSchema(schema);
        }

        public WhereClauseRegisterCondition AppendSchema(string schema) {
            if (AppContext == null) {
                AppContext = new ApplicationLookupContext();
            }
            AppContext.Schema = schema;
            if (Alias == null) {
                Alias = schema;
            }
            return this;
        }

    }
}
