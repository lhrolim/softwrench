using System;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Configuration.Definitions.WhereClause {

    //    [Class(Table = "CONF_WCCONDITION", Lazy = false)]
    [JoinedSubclass(NameType = typeof(WhereClauseCondition), Lazy = false, ExtendsType = typeof(Condition), Table = "CONF_WCCONDITION")]
    public class WhereClauseCondition : Condition {

        //        [Id(0, Name = "WcWcId")]
        //        [Generator(1, Class = "native")]
        [Key(-1,Column = "WcWcId")]
        public virtual int? WcWcId { get; set; }

        [ComponentProperty]
        public ApplicationLookupContext AppContext { get; set; }

        public override ConditionMatchResult MatchesConditions(ConditionMatchResult result,ContextHolder context) {
            if (context == null) {
                return result;
            }
            var superMatches = base.MatchesConditions(result,context);


            return AppContext.MatchesCondition(superMatches, context);


        }
    }
}
