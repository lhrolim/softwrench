﻿using System;
using cts.commons.persistence;
using Iesi.Collections.Generic;
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


        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(ContainerPermission))]
        public ISet<ContainerPermission> ContainerPermissions {
            get; set;
        }

        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(CompositionPermission))]
        public ISet<CompositionPermission> CompositionPermissions {
            get; set;
        }

        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "app_id")]
        [OneToMany(2, ClassType = typeof(ActionPermission))]
        public ISet<ActionPermission> ActionPermissions {
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


        [Property]
        public bool AllowViewOnly {
            get; set;
        }




    }
}
