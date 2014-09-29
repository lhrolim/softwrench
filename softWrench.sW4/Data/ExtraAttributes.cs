using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Data {

    [Class(Table = "sw_extraattributes", Lazy = false)]
    public class ExtraAttributes : IBaseEntity
    {

        public const String ByMaximoTABLEId = "from ExtraAttributes where MaximoTable =? and MaximoId =?";

        public const String ByMaximoTABLEIdAndAttribute = "from ExtraAttributes where MaximoTable =? and MaximoId =? and AttributeName =?";
        //fetching all of this kind
        public const String ByMaximoTABLEAndAttribute = "from ExtraAttributes where MaximoTable =? and AttributeName =?";

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
