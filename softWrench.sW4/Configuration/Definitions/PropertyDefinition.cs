﻿using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Configuration.Util;
using System;
using cts.commons.persistence.Util;
using CompressionUtil = softWrench.sW4.Util.CompressionUtil;

namespace softWrench.sW4.Configuration.Definitions {

    [Class(Table = "CONF_PROPERTYDEFINITION", Lazy = false)]
    public class PropertyDefinition : IComparable<PropertyDefinition> {

        public const string ByKey = "from PropertyDefinition where fullKey=?";

        public PropertyDefinition(string fullKey) {
            FullKey = fullKey;
            Key = CategoryUtil.GetPropertyKey(fullKey);
            Visible = true;
        }

        public PropertyDefinition() {
            DataType = "string";
            Visible = true;
        }

        [Id(0, Name = "FullKey")]
        public virtual string FullKey { get; set; }

        [Property(Column = "key_")]
        public virtual string Key { get; set; }
        [Property]
        [JsonIgnore]
        public virtual string DefaultValue { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property]
        public virtual string DataType { get; set; }

        [Property]
        public virtual string Renderer { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean Visible { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean Contextualized { get; set; }

//        [Property(Type = "BinaryBlob")]
        public virtual byte[] DefaultBlobValue { get; set; }

        [Property(Column = "alias_")]
        public virtual String Alias { get; set; }


        public virtual string StringValue {
            get {
                if (DefaultBlobValue != null) {
                    return StringExtensions.GetString(CompressionUtil.Decompress(DefaultBlobValue));
                }
                return DefaultValue;
            }
            set {
                if (value.Length > 1000) {
                    DefaultBlobValue = CompressionUtil.Compress(value.GetBytes());
                } else {
                    DefaultValue = value;
                }
            }
        }

        public PropertyValue SingleProperty { get; set; }


        //        [ManyToOne(Column = "category_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        //        public virtual Category Category { get; set; }


        [Set(0, Lazy = CollectionLazy.False, Inverse = true)]
        [Key(1, Column = "definition_id")]
        [OneToMany(2, ClassType = typeof(PropertyValue))]
        public virtual ISet<PropertyValue> Values { get; set; }


        public int CompareTo(PropertyDefinition other) {
            return System.String.Compare(Key, other.Key, System.StringComparison.Ordinal);
        }

        public override string ToString() {
            return string.Format("FullKey: {0}, Description: {1}, DataType: {2}, Renderer: {3}", FullKey, Description, DataType, Renderer);
        }

       
    }
}
