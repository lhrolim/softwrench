using System;
using System.Linq;
using softWrench.Mobile.Metadata.Applications.Schema;

namespace softWrench.Mobile.Metadata.Applications
{
    public class ApplicationMetadata : IEquatable<ApplicationMetadata>
    {
        private readonly Guid _id;
        private readonly string _name;
        private readonly string _entity;
        private readonly string _title;
        private readonly string _idFieldName;
        private readonly Lazy<ApplicationField> _idField;
        private readonly MobileApplicationSchema _schema;

        public ApplicationMetadata(Guid id, string name, string title, string entity, string idFieldName, MobileApplicationSchema schema)
        {
            _id = id;
            _name = name;
            _title = title;
            _entity = entity;
            _idFieldName = idFieldName;
            _schema = schema;

            _idField = new Lazy<ApplicationField>(() => _schema
                .Fields
                .First(f => f.Attribute == idFieldName));
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: {1}", Id, Name);
        }

        public bool Equals(ApplicationMetadata other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ApplicationMetadata) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public Guid Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Entity
        {
            get { return _entity; }
        }

        public string Title
        {
            get { return _title; }
        }

        public string IdFieldName
        {
            get { return _idFieldName; }
        }

        public ApplicationField IdField
        {
            get { return _idField.Value; }
        }

        public bool IsUserInteractionEnabled
        {
            get { return _schema.IsUserInteractionEnabled; }
        }

        public MobileApplicationSchema Schema
        {
            get { return _schema; }
        }
    }
}
