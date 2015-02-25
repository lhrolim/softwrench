using cts.commons.persistence;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;
using System;

namespace softWrench.sW4.Configuration.Definitions {

    [Class(Table = "CONF_CONDITION", Lazy = false)]
    public class Condition : IBaseEntity {

        public const string ByAlias = "from Condition where Alias =?";
        public const string GlobalConditions = "from Condition where Global = 1";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property(Column = "alias_")]
        public virtual string Alias { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property]
        public virtual string SiteId { get; set; }

        [Property]
        public virtual string Environment { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public Boolean Global { get; set; }

        [Property]
        public string FullKey { get; set; }

        public virtual ConditionMatchResult MatchesConditions(ConditionMatchResult result, ContextHolder context) {
            return
                result.Append(SiteId, context.SiteId)
                    .Append(ConditionMatchResult.Calculate(Environment, context.Environment));
        }

        public override string ToString() {
            return string.Format("Id: {0}, Alias: {1}, Description: {2}, SiteId: {3}, Environment: {4}, Global: {5}, FullKey: {6}", Id, Alias, Description, SiteId, Environment, Global, FullKey);
        }
    }
}
