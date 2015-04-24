using System;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Configuration.Services.Api {

    public class WhereClauseRegisterCondition : WhereClauseCondition {
        public String Module { get; set; }
        public String UserProfile { get; set; }
        
        public Condition RealCondition {
            get {
                if (Alias == null) {
                    return null;
                }
                if (AppContext == null) {
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
