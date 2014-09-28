using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    class OutputDetailStereotype : ASchemaStereotype {

        private static OutputDetailStereotype _instance;

        public static OutputDetailStereotype GetInstance() {
            return _instance ?? (_instance = new OutputDetailStereotype());
        }


        protected override IDictionary<string, string> DefaultValues() {
            return new Dictionary<string, string>(){
                {ApplicationSchemaPropertiesCatalog.WindowPopupTitleStrategy, "nameandid"},
                {ApplicationSchemaPropertiesCatalog.ShowPrintCommandStrategy, "editonly"},
            };
        }
    }
}
