using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data {

    [Class(Table = "sw_extraattributes", Lazy = false)]
    public class ExtraAttributes : IBaseEntity
    {

        public const String ByMaximoTABLEId = "from ExtraAttributes where MaximoTable =? and MaximoId =?";

        public const String ByMaximoTABLEIdAndAttribute = "from ExtraAttributes where MaximoTable =? and MaximoId =? and AttributeName =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }
        
        [Property]
        public virtual string MaximoTable { get; set; }

        [Property]
        public virtual string MaximoId { get; set; }
        
        [Property]
        public virtual string AttributeName { get; set; }
                              
        [Property]
        public virtual string AttributeValue { get; set; }


    }
}
