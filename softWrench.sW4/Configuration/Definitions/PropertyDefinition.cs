using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Configuration.Util;
using System;
using System.Collections.Generic;
using cts.commons.persistence.Util;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softwrench.sw4.api.classes.configuration;
using CompressionUtil = softWrench.sW4.Util.CompressionUtil;
using NHibernate.UserTypes;
using NHibernate.SqlTypes;
using System.Data;
using System.Threading.Tasks;
using NHibernate;

namespace softWrench.sW4.Configuration.Definitions {

    [Class(Table = "CONF_PROPERTYDEFINITION", Lazy = false)]
    public class PropertyDefinition : IComparable<PropertyDefinition> {

        public const string ByKey = "from PropertyDefinition where FullKey=?";
        public const string MultipleByKey = "from PropertyDefinition where FullKey in (:p0)";
        public const string ByVisibilityByConfigTypeOrderedByKey = "from PropertyDefinition where Visible = ? and FullKey like ? order by FullKey asc";
        public const string ByCachedOnClient = "from PropertyDefinition where CachedOnClient=?";

        public PropertyDefinition(string fullKey) {
            FullKey = fullKey;
            SimpleKey = CategoryUtil.GetPropertyKey(fullKey);
            Visible = true;
        }

        public PropertyDefinition() {
            DataType = PropertyDataType.STRING.ToString().ToLower();
            Visible = true;
        }

        [Id(0, Name = "FullKey")]
        public virtual string FullKey {
            get; set;
        }

        [Property(Column = "key_")]
        public virtual string SimpleKey {
            get; set;
        }

        [Property]
        [JsonIgnore]
        public virtual string DefaultValue {
            get; set;
        }

        [Property]
        public virtual string Description {
            get; set;
        }

        /// <summary>
        /// Enum Data representing the datatype of the <see cref="PropertyDefinition"/>
        /// Available options: string, int, long, date, boolean.
        /// The information would be presented on screen using the datatype.
        /// </summary>
        public virtual PropertyDataType PropertyDataType {
            get {
                PropertyDataType outdata;
                if(Enum.TryParse(DataType, true, out outdata)) {
                    return outdata;
                }

                return PropertyDataType.STRING;
            }
            set {
                DataType = value.ToString().ToLower();
            }
        }

        /// <summary>
        /// DO NOT USE THIS PROPERTY TO SET THE DATATYPE 
        /// Use <see cref="PropertyDataType"/> Enum property to define the datatype for this property definition
        /// Available options: long, string, date, boolean --> the way the information would be presented on screen
        /// </summary>
        [Property]
        public virtual string DataType {
            get; set;
        }

        [Property]
        public virtual string Renderer {
            get; set;
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual bool Visible {
            get; set;
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual bool Contextualized {
            get; set;
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual bool CachedOnClient {
            get; set;
        }

        //        [Property(Type = "BinaryBlob")]
        public virtual byte[] DefaultBlobValue {
            get; set;
        }

        [Property(Column = "alias_")]
        public virtual string Alias {
            get; set;
        }

        [Property(Column = "minvalue_")]
        public virtual string MinValue_ {
            get; set;
        }

        [Property(Column = "maxvalue_")]
        public virtual string MaxValue_ {
            get; set;
        }

        public virtual string StringValue {
            get {
                if (DefaultBlobValue != null) {
                    return StringExtensions.GetString(CompressionUtil.Decompress(DefaultBlobValue));
                }
                return DefaultValue;
            }
            set {
                if (value != null && value.Length > 1000) {
                    DefaultBlobValue = CompressionUtil.Compress(value.GetBytes());
                } else {
                    DefaultValue = value;
                }
            }
        }

        public PropertyValue SingleProperty {
            get; set;
        }


        //        [ManyToOne(Column = "category_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        //        public virtual Category Category { get; set; }


        [Set(0, Lazy = CollectionLazy.False, Inverse = true)]
        [Key(1, Column = "definition_id")]
        [OneToMany(2, ClassType = typeof(PropertyValue))]
        public virtual ISet<PropertyValue> Values {
            get; set;
        }


        public int CompareTo(PropertyDefinition other) {
            return string.Compare(SimpleKey, other.SimpleKey, System.StringComparison.Ordinal);
        }

        public override string ToString() {            
            return string.Format("FullKey: {0}, Description: {1}, DataType: {2}, Renderer: {3}", FullKey, Description, DataType, Renderer);           
        }        
    }

    /// <summary>
    /// The datatype enum for the <see cref="PropertyDefinition" class./>
    /// </summary>
    public enum PropertyDataType {
        STRING = 0,
        INT = 1,
        LONG = 2,
        DATE = 3,
        BOOLEAN = 4
    }
}
