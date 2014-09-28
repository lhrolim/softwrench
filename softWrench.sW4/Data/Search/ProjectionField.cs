using System;

namespace softWrench.sW4.Data.Search {

    /// <summary>
    /// Specify a field to be included in a given query projection.
    /// </summary>
    public class ProjectionField {
        private string _alias;

        public string Name { get; set; }

        public ProjectionField(string @alias, string name) {
            _alias = alias;
            Name = name;
        }
        protected bool Equals(ProjectionField other)
        {
            return string.Equals(_alias, other._alias) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectionField)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_alias != null ? _alias.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public ProjectionField() {

        }

        public static ProjectionField Default(String value) {
            return new ProjectionField{
                _alias = value,
                Name = value
            };
        }

        public string Alias {
            //removing . from aliases as it breaks on db2
            get { return _alias.IndexOf('.') == -1 ? _alias : _alias.Replace(".", String.Empty); }
            set { _alias = value; }
        }

        public override string ToString() {
            return string.Format("Name: {0}, Alias: {1}", Name, Alias);
        }

    }
}
