using cts.commons.portable.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Context;
using System;
using CompressionUtil = softWrench.sW4.Util.CompressionUtil;

namespace softWrench.sW4.Configuration.Definitions {

    [Class(Table = "CONF_PROPERTYVALUE", Lazy = false)]
    public class PropertyValue {

        public const string ByCondition = "from PropertyValue where Condition = ?";

        public const string ByDefinitionConditionModuleProfile = "from PropertyValue v where Definition.FullKey = ? and Condition = ? and Module = ? and UserProfile = ?";

        public const string DistinctModules = "select distinct(Module) from PropertyValue";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        [JsonIgnore]
        public virtual string Value { get; set; }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] BlobValue { get; set; }



        [Newtonsoft.Json.JsonIgnore]
        [ManyToOne(Column = "definition_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual PropertyDefinition Definition { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [ManyToOne(Column = "condition_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual Condition Condition { get; set; }

        [Property]
        public virtual string Module { get; set; }

        [Property]
        public virtual int? UserProfile { get; set; }


        [Property]
        [JsonIgnore]
        public virtual string SystemValue { get; set; }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] SystemBlobValue { get; set; }

        public virtual string SystemStringValue {
            get {
                return SystemBlobValue != null ? StringExtensions.GetString(CompressionUtil.Decompress(SystemBlobValue)) : SystemValue;
            }
            set {
                if (value.Length > 1000) {
                    SystemBlobValue = CompressionUtil.Compress(value.GetBytes());
                } else {
                    SystemValue = value;
                }
            }
        }

        public virtual int? ConditionId {
            get { return Condition == null ? null : Condition.Id; }
        }

        public virtual string StringValue {
            get {
                return BlobValue != null ? StringExtensions.GetString(CompressionUtil.Decompress(BlobValue)) : Value;
            }
            set {
                if (value.Length > 1000) {
                    BlobValue = CompressionUtil.Compress(value.GetBytes());
                } else {
                    Value = value;
                }
            }
        }

        public virtual ConditionMatchResult MatchesConditions(ContextHolder context) {
            var result = new ConditionMatchResult(Module, UserProfile);
            result.AppendModule(Module, context.Module);
            result.AppendSelectedProfile(UserProfile, context.CurrentSelectedProfile);
            result.AppendAvailableProfile(UserProfile, context.UserProfiles);
            context.MatchesCondition(Condition, result);
            return result;
        }

        private bool NullOrEqual(String conditionString, String contextString) {
            return conditionString == null || conditionString.Equals(contextString, StringComparison.CurrentCultureIgnoreCase);
        }


        protected bool Equals(PropertyValue other) {
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyValue)obj);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override string ToString() {
            return string.Format("UserProfile: {0}, Module: {1}, Condition: {{{2}}}, Id: {3}, SystemValue: {4}", UserProfile, Module, Condition, Id, StringValue);
        }
    }
}
