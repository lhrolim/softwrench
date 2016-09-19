using System.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Configuration.Definitions {

    [Class(Table = "CONF_CATEGORY", Lazy = false)]
    public class Category {


        public const string ByKey = "from Category where key=? and parentCategory.key=?";

        public Category() {

        }

        //        [Id(0, Name = "Id")]
        //        [Generator(1, Class = "native")]
        //        public virtual int? Id { get; set; }

        [Id(0, Name = "FullKey")]
        public virtual string FullKey { get; set; }

        [Property(Column = "key_")]
        public virtual string Key { get; set; }



        [Property]
        public virtual string Description { get; set; }

        [ManyToOne(Column = "parent_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "all")]
        public virtual Category ParentCategory { get; set; }


        [Set(0, Inverse = true,Lazy = CollectionLazy.False)]
        [Key(1, Column = "category_id")]
        [OneToMany(2, ClassType = typeof(PropertyDefinition))]
        public virtual ISet<PropertyDefinition> Definitions { get; set; }

        protected bool Equals(Category other) {
            return string.Equals(Key, other.Key) && Equals(ParentCategory, other.ParentCategory);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Category)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (ParentCategory != null ? ParentCategory.GetHashCode() : 0);
            }
        }


        public override string ToString() {
            return string.Format("Key: {0}, Fullkey: {1}", Key, FullKey);
        }

        public virtual SortedSet<Category> Children { get; set; }

        //        public string FullKey() {
        //            string fullKey = Key + "/";
        //            var pCategory = ParentCategory;
        //            while (pCategory != null) {
        //                fullKey = "/" + ParentCategory.Key + "/" + fullKey;
        //                pCategory = pCategory.ParentCategory;
        //            }
        //            return fullKey;
        //        }


    }
}
