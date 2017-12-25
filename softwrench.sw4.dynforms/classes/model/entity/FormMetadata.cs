using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using JetBrains.Annotations;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Metadata.Validator;

namespace softwrench.sw4.dynforms.classes.model.entity {

    [Class(Table = "FORM_METADATA", Lazy = false)]
    public class FormMetadata {

        [Id(0, Name = "Name")]
        public virtual string Name { get; set; }

        [Property]
        public virtual string Entity { get; set; }

        [Property(Column = "title")]
        [UserIdProperty]
        public virtual string FormTitle { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property(Column = "status")]
        public virtual string FormStatus { get; set; }

        [Property]
        public virtual DateTime? ChangeDate { get; set; }


        [OneToOne(ClassType = typeof(FormMetadataDefinition), Lazy = Laziness.False, PropertyRef = "Metadata", Cascade = "none")]
        public virtual FormMetadataDefinition Definition {
            get; set;
        }

        public FormMetadata() {

        }

        public FormMetadata(string name) {
            Name = name;
        }

        public FormMetadata(string name, string title) {
            Name = name;
            FormTitle = title;
        }

    }
}
