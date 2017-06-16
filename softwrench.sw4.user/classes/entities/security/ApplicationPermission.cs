using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.web.Formatting;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Util;

namespace softwrench.sw4.user.classes.entities.security {

    /// <summary>
    /// Controls basic access to a given application.
    /// 
    /// Only the applications on the menu, or the ones which are explictely marked as toplevel would require such a permission.
    /// 
    /// If none of the flags of the CollectionPermission are checked, only read-only access would be granted
    /// 
    /// </summary>
    [Class(Table = "SEC_APPLICATION_PER", Lazy = false)]
    public class ApplicationPermission : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }


        [Property]
        public string ApplicationName {
            get; set;
        }


        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(ContainerPermission))]
        [JsonConverter(typeof(IesiSetConverter<ContainerPermission>))]
        public ISet<ContainerPermission> ContainerPermissions {
            get; set;
        }

        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(CompositionPermission))]
        [JsonConverter(typeof(IesiSetConverter<CompositionPermission>))]
        public ISet<CompositionPermission> CompositionPermissions {
            get; set;
        }

        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(ActionPermission))]
        [JsonConverter(typeof(IesiSetConverter<ActionPermission>))]
        public ISet<ActionPermission> ActionPermissions {
            get; set;
        }

        [JsonIgnore]
        [ManyToOne(Column = "profile_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public UserProfile Profile {
            get; set;
        }


        [Property]
        public bool AllowCreation {
            get; set;
        }

        [Property]
        public bool AllowUpdate {
            get; set;
        }


        [Property]
        public bool AllowRemoval {
            get; set;
        }

        /// <summary>
        /// Whether or not the application has READ ONLY permission.
        /// </summary>
        [Property]
        public bool AllowView {
            get; set;
        }

        public bool HasNoPermissions => !AllowCreation && !AllowUpdate && !AllowView;

        public bool AllDefault => HasNoPermissions;

        public bool HasContainerPermissionOfSchema(string schema) {
            return ContainerPermissions != null && ContainerPermissions.Any(c => c.Schema.EqualsIc(schema));
        }


        public void Merge(ApplicationPermission other) {
            AllowCreation = AllowCreation || other.AllowCreation;
            AllowUpdate = AllowCreation || other.AllowUpdate;
            AllowView = AllowCreation || other.AllowView;
            AllowRemoval = AllowCreation || other.AllowRemoval;
            if (ActionPermissions == null) {
                ActionPermissions = new LinkedHashSet<ActionPermission>();
            }
            if (CompositionPermissions == null) {
                CompositionPermissions = new LinkedHashSet<CompositionPermission>();
            }

            if (ContainerPermissions == null) {
                ContainerPermissions = new LinkedHashSet<ContainerPermission>();
            }
            if (other.ActionPermissions == null) {
                other.ActionPermissions = new LinkedHashSet<ActionPermission>();
            }

            if (other.ContainerPermissions == null) {
                other.ContainerPermissions = new LinkedHashSet<ContainerPermission>();
            }

            if (other.CompositionPermissions == null) {
                other.CompositionPermissions = new LinkedHashSet<CompositionPermission>();
            }


            //action is simple, just add them all
            ActionPermissions.AddAll(other.ActionPermissions);

            foreach (var containerPermission in other.ContainerPermissions) {
                var thisContainer = ContainerPermissions.FirstOrDefault(
                    f =>
                        f.Schema.EqualsIc(containerPermission.Schema) &&
                        f.ContainerKey.EqualsIc(containerPermission.ContainerKey));
                if (thisContainer == null) {
                    ContainerPermissions.Add(containerPermission);
                } else {
                    thisContainer.Merge(containerPermission);
                }
            }


            foreach (var otherComposition in other.CompositionPermissions) {
                var thisComposition = CompositionPermissions.FirstOrDefault(
                    f =>
                        f.Schema.EqualsIc(otherComposition.Schema) &&
                        f.CompositionKey.EqualsIc(otherComposition.CompositionKey));
                if (thisComposition == null) {
                    CompositionPermissions.Add(otherComposition);
                } else {
                    thisComposition.Merge(otherComposition);
                }
            }

        }

        protected bool Equals(ApplicationPermission other) {
            return string.Equals(ApplicationName, other.ApplicationName) && Equals(Profile.Id, other.Profile.Id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationPermission)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((ApplicationName != null ? ApplicationName.GetHashCode() : 0) * 397) ^ (Profile != null ? Profile.GetHashCode() : 0);
            }
        }

        public override string ToString() {
            return
                $"ApplicationName: {ApplicationName}, AllowCreation: {AllowCreation}, AllowUpdate: {AllowUpdate}, AllowView: {AllowView}";
        }
    }
}
