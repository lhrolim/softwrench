using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Dynamic.Model {

    [Class(Table = "DYN_SCRIPT_ENTRY", Lazy = false)]
    public class ScriptEntry : AScriptEntry {

        public const string ScriptByTargetDeployVersion = "from ScriptEntry where Target = ? and Deploy = ? and Appliestoversion = ?";
        public const string ScriptByDeployVersion = "from ScriptEntry where Deploy = ? and Appliestoversion = ?";
        public const string ScriptByName = "from ScriptEntry where Name = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public override bool Isoncontainer {
            get; set;
        }

        [Property]
        public bool Isuptodate {
            get; set;
        }

        public string Status {
            get; set;
        } 



        public override AScriptEntry ShallowCopy() {
            return (ScriptEntry)MemberwiseClone();
        }
    }
}
