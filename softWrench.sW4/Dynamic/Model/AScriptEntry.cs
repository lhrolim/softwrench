using System;
using cts.commons.persistence.Util;
using NHibernate.Mapping.Attributes;
using NHibernate.SqlTypes;
using NHibernate.Type;
using softWrench.sW4.Util;

namespace softWrench.sW4.Dynamic.Model {
    public abstract class AScriptEntry {

        //        [Id(0, Name = "Id")]
        //        [Generator(1, Class = "native")]
        //        public int? Id {
        //            get; set;
        //        }

        [Property]
        public string Name {
            get; set;
        }

        [Property]
        public string Target {
            get; set;
        }

        [Property]
        public string Description {
            get; set;
        }

        [Property(Length = Int32.MaxValue)]
        public string Script {
            get; set;
        }

        [Property]
        public long Lastupdate {
            get; set;
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public bool Deploy {
            get; set;
        }

        [Property]
        public string Appliestoversion {
            get; set;
        }

        public string Comment {
            get; set;
        }

        public string Username {
            get; set;
        }

        public abstract bool Isoncontainer { get; set;}

        public abstract AScriptEntry ShallowCopy();



        public override string ToString() {
            return $"Target: {Target}, Lastupdate: {DateExtensions.FromUnixTimeStamp(Lastupdate)}";
        }
    }
}
