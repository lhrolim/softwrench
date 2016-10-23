using cts.commons.persistence;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Context;
using System;
using cts.commons.persistence.Util;

namespace softWrench.sW4.Configuration.Definitions {

    [Class(Table = "CONF_CONDITION", Lazy = false)]
    public class Condition : IBaseEntity {

        public const string ByAlias = "from Condition where Alias =?";
        public const string GlobalConditions = "from Condition where Global = 1";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id {
            get; set;
        }

        /// <summary>
        ///  This is an identifier of the condition, that should be used in order to allow the framework to update the conditions upon server restarts. 
        ///  If an Alias is not provided then the condition won´t be persisted
        /// </summary>
        [Property(Column = "alias_")]
        public virtual string Alias {
            get; set;
        }

        [Property]
        public virtual string Description {
            get; set;
        }

        [Property]
        public virtual string SiteId {
            get; set;
        }

        [Property]
        public virtual string Environment {
            get; set;
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public Boolean Global {
            get; set;
        }

        /// <summary>
        /// this points to the category fullkey
        /// </summary>
        [Property]
        public string FullKey {
            get; set;
        }

        public virtual ConditionMatchResult MatchesConditions(ConditionMatchResult result, ContextHolder context) {
            return
                result.Append(SiteId, context.SiteId)
                    .Append(ConditionMatchResult.Calculate(Environment, context.Environment));
        }

        protected bool Equals(Condition other) {
            return string.Equals(SiteId, other.SiteId) && string.Equals(Environment, other.Environment) && Global == other.Global && string.Equals(FullKey, other.FullKey);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Condition)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (SiteId != null ? SiteId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Global.GetHashCode();
                hashCode = (hashCode * 397) ^ (FullKey != null ? FullKey.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Alias: {1}, Description: {2}, SiteId: {3}, Environment: {4}, Global: {5}, FullKey: {6}", Id, Alias, Description, SiteId, Environment, Global, FullKey);
        }
    }
}
