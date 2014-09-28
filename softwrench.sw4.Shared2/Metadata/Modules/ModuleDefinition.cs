using System;

namespace softwrench.sw4.Shared2.Metadata.Modules {
    public class ModuleDefinition : IComparable<ModuleDefinition> {
        public ModuleDefinition(string id, string @alias) {
            Id = id;
            Alias = alias;
            if (alias == null) {
                Alias = id;
            }
        }

        public ModuleDefinition() {

        }

        public string Id { get; set; }
        public string Alias { get; set; }


        public int CompareTo(ModuleDefinition other) {
            return System.String.Compare(Alias, other.Alias, System.StringComparison.Ordinal);
        }

        protected bool Equals(ModuleDefinition other) {
            return string.Equals(Id, other.Id,StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModuleDefinition)obj);
        }

        public override int GetHashCode() {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}
