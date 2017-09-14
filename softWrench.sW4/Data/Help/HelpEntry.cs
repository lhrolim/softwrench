using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Help {


    [Class(Table = "HELP_ENTRY", Lazy = false)]
    public class HelpEntry :IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }


        //TODO:Label breaks grid css
        [Property(Column = "label")]
        public virtual string EntryLabel {
            get; set;
        }

        [Property]
        public virtual string DocumentName {
            get; set;
        }

        [Property(Column = "type_")]
        public virtual string Type {
            get; set;
        }

        [Property]
        public virtual string Url {
            get; set;
        }

        [Property(Type = "BinaryBlob")]
        public virtual byte[] Data {
            get; set;
        }





    }
}
