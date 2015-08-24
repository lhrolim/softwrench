namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntityTargetConstant {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }

        protected bool Equals(EntityTargetConstant other) {
            return string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityTargetConstant)obj);
        }

        public override int GetHashCode() {
            return (Key != null ? Key.GetHashCode() : 0);
        }
    }
}
