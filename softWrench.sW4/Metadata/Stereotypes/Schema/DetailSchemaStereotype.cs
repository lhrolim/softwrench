using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    class DetailSchemaStereotype : ASchemaStereotype {

        private static DetailSchemaStereotype _instance;

        public static DetailSchemaStereotype GetInstance() {
            return _instance ?? (_instance = new DetailSchemaStereotype());
        }

        protected DetailSchemaStereotype() { }



        protected override IDictionary<string, string> DefaultValues() {
            return new Dictionary<string, string>()
            {
                {NextSchemaId, "list"},
                {ApplicationSchemaPropertiesCatalog.WindowPopupTitleStrategy, "nameandid"},
                {ApplicationSchemaPropertiesCatalog.ShowPrintCommandStrategy, "editonly"},
            };
        }
    }
}
