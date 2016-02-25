using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.web.Formatting;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

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
        public Iesi.Collections.Generic.ISet<ContainerPermission> ContainerPermissions {
            get; set;
        }

        [Set(0, Lazy = CollectionLazy.False, Cascade = "all-delete-orphan")]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(CompositionPermission))]
        [JsonConverter(typeof(IesiSetConverter<CompositionPermission>))]
        public Iesi.Collections.Generic.ISet<CompositionPermission> CompositionPermissions {
            get; set;
        }

        [Set(0, Lazy = CollectionLazy.False, Cascade = "all-delete-orphan")]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(ActionPermission))]
        [JsonConverter(typeof(IesiSetConverter<ActionPermission>))]
        public Iesi.Collections.Generic.ISet<ActionPermission> ActionPermissions {
            get; set;
        }

        [JsonIgnore]
        [ManyToOne(Column = "profile_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "all")]
        public UserProfile Profile { get; set; }


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


        [Property]
        public bool AllowViewOnly {
            get; set;
        }

        public bool HasNoPermissions {
            //TODO: add AllowRemoval later...
            get { return !AllowCreation && !AllowUpdate && !AllowViewOnly; }
        }


        public void Merge(ApplicationPermission other) {
            AllowCreation = AllowCreation && other.AllowCreation;
            AllowUpdate = AllowCreation && other.AllowUpdate;
            AllowViewOnly = AllowCreation && other.AllowViewOnly;
            AllowRemoval = AllowCreation && other.AllowRemoval;
            if (ActionPermissions == null) {
                ActionPermissions = new HashedSet<ActionPermission>();
            }
            if (CompositionPermissions == null) {
                CompositionPermissions = new HashedSet<CompositionPermission>();
            }

            if (ContainerPermissions == null) {
                ContainerPermissions = new HashedSet<ContainerPermission>();
            }
            if (other.ActionPermissions == null) {
                other.ActionPermissions = new HashedSet<ActionPermission>();
            }

            if (other.ContainerPermissions == null) {
                other.ContainerPermissions = new HashedSet<ContainerPermission>();
            }

            if (other.CompositionPermissions== null) {
                other.CompositionPermissions = new HashedSet<CompositionPermission>();
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
    }
}
