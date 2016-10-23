using System;
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

        public Condition RealCondition {
            get {
                if (Alias == null) {
                    //check documentation of Alias --> no alias means the framework can´t locate the existing condition to perform an update
                    return null;
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
