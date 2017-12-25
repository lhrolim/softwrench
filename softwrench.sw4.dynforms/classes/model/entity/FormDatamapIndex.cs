using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dynforms.classes.model.entity {

    [Class(Table = "FORM_DATAMAP_IDX", Lazy = false)]
    public class FormDatamapIndex : IBaseEntity{

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [ManyToOne(ClassType = typeof(FormDatamap), Column = "datamap_id")]
        public FormDatamap FormDataMap { get; set; }

        [Property]
        public string AttributeName { get; set; }

        [ManyToOne(Column = "form_name", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "all")]
        public FormMetadata Metadata { get; set; }

        [Property(Column = "value_")]
        public string Value { get; set; }

        [Property]
        public int NumValue { get; set; }

        [Property]
        public DateTime? DateValue { get; set; }


    }
}
