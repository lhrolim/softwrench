using cts.commons.portable.Util;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {

    public class BaseMetadataFilter {
        public BaseMetadataFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, bool remove = false) {
            Attribute = attribute;
            Label = label;
            Icon = icon;
            Position = position;
            Tooltip = tooltip;
            WhereClause = whereClause;
            Remove = remove;
        }

        public virtual string Type {
            get {
                return GetType().Name;
            }
        }

        /// <summary>
        /// based upon the type of the column, this will serve to the framework as a hint of which among the operations can be performed
        /// </summary>
        public string RendererType {
            get; set;
        }

        public bool Remove {
            get; set;
        }

        public string Attribute {
            get; set;
        }
        public string Label {
            get; set;
        }

        public string Icon {
            get; set;
        }

        public string Position {
            get; set;
        }

        public string Tooltip {
            get; set;
        }

        /// <summary>
        /// This defines how the filter should impact the execution query. Usually, the filter will simply append to the attribute field, but sometimes a query can combine multiple fields, or be a complex expression
        /// 
        /// The whereclause can be:
        /// 
        /// 1. a vanilla string using !@ as a placeholder for the current entity
        /// 2. a @xxx.yyy string that will be evaluated to either a SimpleInjector(online) or a service method (mobile)
        /// 
        /// Note: There´s no way to validate whereclauses of type 2 on server side unless the schema is marked as web
        /// 
        /// </summary>
        public string WhereClause {
            get; set;
        }

        public bool IsTransient() {
            return Attribute.StartsWith("#");
        }


        protected bool Equals(BaseMetadataFilter other) {
            return string.Equals(Attribute, other.Attribute);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseMetadataFilter)obj);
        }

        public override string ToString() {
            return string.Format("Attribute: {0}, Label: {1}", Attribute, Label);
        }

        public override int GetHashCode() {
            return (Attribute != null ? Attribute.GetHashCode() : 0);
        }

        public virtual bool IsValid() {

            if (Remove || Position != null) {
                //this is a removal, or a cross schema customization filter
                return true;
            }

            //transient filters do not need whereclauses since the filter will be applied on the attribute
            var hasValidWhereClause = !IsTransient() || WhereClause != null;

            return (Attribute != null && Label != null && hasValidWhereClause);
        }

        #region utility methods
        public static BaseMetadataFilter FromField(string attribute, string label, string tooltip, string type) {
            if (type.EqualsAny("datetime", "timestamp")) {
                return new MetadataDateTimeFilter(attribute, label, null, null, tooltip, null, true, false);
            }
            if (type.EqualsAny("boolean")) {
                return new MetadataBooleanFilter(attribute, label, null, null, tooltip, null, true);
            }

            if (type.EqualsAny("smallint", "int", "integer", "decimal", "float", "double")) {
                //TODO: allow to customize numeric extensions
                return new MetadataNumberFilter(attribute, label, null, null, tooltip, null);
            }

            return new BaseMetadataFilter(attribute, label, null, null, tooltip, null);
        }
        #endregion


    }
}
