using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {


    [Class(Table = "SEC_SECTION_PER", Lazy = false)]
    public class SectionPermission : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }


        [Property]
        public string Permission {
            get; set;
        }

        public bool ReadOnly => Permission.Equals("readonly");
        public bool FullControl => Permission.Equals("fullcontrol");
      

        public bool AnyPermission => Permission!= "none";

        [Property]
        public string SectionId {
            get; set;
        }


        public void Merge(SectionPermission otherField) {
            if (Permission.EqualsIc("none")) {
                //if we are on none, let´s change to the other permission that will be more permissive.
                Permission = otherField.Permission;
            } else if (Permission.EqualsIc("readonly")) {
                Permission = otherField.Permission == "fullcontrol" ? "fullcontrol" : Permission;
            }
        }


        public override string ToString() {
            return $"{nameof(ReadOnly)}: {ReadOnly}, {nameof(FullControl)}: {FullControl}, {nameof(SectionId)}: {SectionId}";
        }
    }
}
