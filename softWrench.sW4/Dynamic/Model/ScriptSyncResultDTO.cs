using System;
using JetBrains.Annotations;

namespace softWrench.sW4.Dynamic.Model {

    /// <summary>
    /// Represents a synchronization result for a given script. There are some scenarios to handle:
    /// 
    /// 1) Script exists on client side and on server side with same rowstamp --> just the name will be set
    /// 2) Script exists on client side and on server side, but with a newer version on server --> name, code and upDateTime
    /// 3) Script exists only on server side--> name, code and upDateTime
    /// 4) Script exists on client but not on server --> name and toDelete=true
    /// 
    /// </summary>
    public class ScriptSyncResultDTO {

        public string Target {
            get; set;
        }

        [CanBeNull]
        public string Code {
            get; set;
        }

        public long Rowstamp {
            get; set;
        }

        public bool ToDelete {
            get; set;
        }

        public override string ToString() {
            return $"Target: {Target}, UpDateTime: {Rowstamp}";
        }

        protected bool Equals(ScriptSyncResultDTO other) {
            return string.Equals(Target, other.Target);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((ScriptSyncResultDTO)obj);
        }

        public static ScriptSyncResultDTO FromEntity(JavascriptEntry deployedScript) {
            return new ScriptSyncResultDTO {
                Target = deployedScript.Target,
                Code = deployedScript.Script,
                Rowstamp = deployedScript.Lastupdate
            };
        }

        public override int GetHashCode() {
            return Target?.GetHashCode() ?? 0;
        }
    }
}
