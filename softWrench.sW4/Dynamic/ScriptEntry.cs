using System;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Dynamic {

    [Class(Table = "DYN_SCRIPT_ENTRY", Lazy = false)]
    public class ScriptEntry : IBaseEntity {

        public const string ScriptByTargetDeployVersion = "from ScriptEntry where Target = ? and Deploy = ? and Appliestoversion = ?";
        public const string ScriptByDeployVersion = "from ScriptEntry where Deploy = ? and Appliestoversion = ?";
        public const string ScriptByName = "from ScriptEntry where Name = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

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

        [Property]
        public string Script {
            get; set;
        }

        [Property]
        public DateTime Lastupdate {
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

        [Property]
        public bool Isoncontainer {
            get; set;
        }

        [Property]
        public bool Isuptodate {
            get; set;
        }

        public string Status {
            get; set;
        } 

        public string Comment {
            get; set;
        }

        public string Username {
            get; set;
        }

        public ScriptEntry ShallowCopy() {
            return (ScriptEntry)MemberwiseClone();
        }
    }
}
