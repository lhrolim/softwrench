

using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Util;
using System.Collections.Generic;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    internal class ImacConstants {

        public static SortedSet<IAssociationOption> DefaultTemplateOptions = new SortedSet<IAssociationOption>(){
                new AssociationOption(Add,"Add Subcomponent"),
                new AssociationOption(Install,"Install"),
                new AssociationOption(Remove,"Remove"),
                new AssociationOption(Replace,"Replace"),
                new AssociationOption(Update,"Update"),
                new AssociationOption(Move,"Move"),
            };



        internal const string Add = "add";
        internal const string Install = "install";

        internal const string InstallOther = "installother";
        internal const string InstallStd = "installstd";
        internal const string InstallLan = "installlan";

        internal const string Remove = "remove";
        internal const string RemoveOther = "removeother";
        internal const string RemoveStd = "removestd";
        internal const string RemoveLan = "removelan";


        internal const string Decomissioned = "decomission";
        internal const string Replace = "replace";
        internal const string ReplaceOther = "replaceother";
        internal const string ReplaceStd = "replacestd";
        internal const string ReplaceLan = "replacelan";

        internal const string Move = "move";
        internal const string Update = "update";

        internal static bool IsStdSchema(string schema) {
            return schema.EqualsAny(InstallStd, ReplaceStd, RemoveStd);
        }


        public static bool IsLanSchema(string schema) {
            return schema.EqualsAny(InstallLan, ReplaceLan, RemoveLan);
        }

        public static bool IsOtherSchema(string schema) {
            return schema.EqualsAny(InstallOther, ReplaceOther, RemoveOther);
        }
    }
}
