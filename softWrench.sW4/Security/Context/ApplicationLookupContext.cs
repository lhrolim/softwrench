using NHibernate.Mapping.Attributes;
using softWrench.sW4.Configuration.Definitions;
using con = softWrench.sW4.Configuration.Definitions.ConditionMatchResult;

namespace softWrench.sW4.Security.Context {

    [Component]
    public class ApplicationLookupContext {

        [Property]
        public virtual string Mode { get; set; }

        [Property(Column = "schema_")]
        public virtual string Schema { get; set; }

        [Property]
        public virtual string ParentApplication { get; set; }


        [Property]
        public virtual string ParentSchema { get; set; }

        [Property]
        public virtual string ParentMode { get; set; }


        [Property]
        public virtual string AttributeName { get; set; }

        [Property]
        public virtual string MetadataId { get; set; }

       

        public ConditionMatchResult MatchesCondition(ConditionMatchResult result, ContextHolder context) {
            var c = context.ApplicationLookupContext;
            if (c == null) {
                c = new ApplicationLookupContext();
            }
            return result.AppendMetadataMatch(MetadataId, c.MetadataId)
                .Append(con.Calculate(Schema, c.Schema))
                .Append(con.Calculate(Mode, c.Mode))
                .Append(con.Calculate(ParentApplication, c.ParentApplication))
                .Append(con.Calculate(ParentSchema, c.ParentSchema))
                .Append(con.Calculate(ParentMode, c.ParentMode))
                .Append(con.Calculate(AttributeName, c.AttributeName));
            // OfflineOnly == false means that the condition should be avilable to both online and offline modes.
            //.Append(con.Calculate(OfflineOnly ? OfflineOnly.ToString() : null, c.OfflineOnly.ToString()));
        }

        private bool NullOrEqual(string conditionString, string contextString) {
            return conditionString == null || conditionString == contextString;
        }

        protected bool Equals(ApplicationLookupContext other)
        {
            return string.Equals(Mode, other.Mode) && string.Equals(Schema, other.Schema)
                   && string.Equals(ParentApplication, other.ParentApplication) &&
                   string.Equals(ParentSchema, other.ParentSchema)
                   && string.Equals(AttributeName, other.AttributeName) && string.Equals(ParentMode, other.ParentMode) && string.Equals(MetadataId, other.MetadataId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationLookupContext)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (Mode != null ? Mode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Schema != null ? Schema.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ParentApplication != null ? ParentApplication.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ParentSchema != null ? ParentSchema.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AttributeName != null ? AttributeName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ParentMode != null ? ParentMode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MetadataId != null ? MetadataId.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("Mode: {0}, Schema: {1}, ParentApplication: {2}, ParentSchema: {3}, ParentMode: {4}, AttributeName: {5}, MetadataId: {6}",
                Mode, Schema, ParentApplication, ParentSchema, ParentMode, AttributeName, MetadataId);
        }

        //        private bool NullContext(ApplicationLookupContext c) {
        //            return (c != null) || (c == null && MetadataId == null && Schema == null && Mode == null && ParentApplication == null && ParentSchema == null && ParentMode == null && AttributeName == null);
        //        }

    }
}
