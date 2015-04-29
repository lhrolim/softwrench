﻿using System;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softWrench.sW4.Configuration.Definitions.WhereClause {

    //    [Class(Table = "CONF_WCCONDITION", Lazy = false)]
    [JoinedSubclass(NameType = typeof(WhereClauseCondition), Lazy = false, ExtendsType = typeof(Condition), Table = "CONF_WCCONDITION")]
    public class WhereClauseCondition : Condition {

        //        [Id(0, Name = "WcWcId")]
        //        [Generator(1, Class = "native")]
        [Key(-1, Column = "WcWcId")]
        public virtual int? WcWcId { get; set; }

        [ComponentProperty]
        public ApplicationLookupContext AppContext { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public bool OfflineOnly { get; set; }

        public override ConditionMatchResult MatchesConditions(ConditionMatchResult result, ContextHolder context) {
            if (context == null) {
                return result;
            }
            var superMatches = base.MatchesConditions(result, context);
            // OfflineOnly == false means that the condition should be avilable to both online and offline modes.
            superMatches = superMatches.Append(ConditionMatchResult.Calculate(OfflineOnly ? OfflineOnly.ToString() :
                null, context.OfflineMode.ToString()));
            if (AppContext == null) {
                return superMatches;
            }
            return AppContext.MatchesCondition(superMatches, context);


        }
    }
}
