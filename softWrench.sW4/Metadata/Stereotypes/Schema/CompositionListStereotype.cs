using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    class CompositionListStereotype : ASchemaStereotype {

        private static CompositionListStereotype _instance;

        public static string ShowMainButtos = "composition.mainbuttonstoshow";

        public static CompositionListStereotype GetInstance() {
            return _instance ?? (_instance = new CompositionListStereotype());
        }

        private CompositionListStereotype() { }

        protected override IDictionary<string, string> DefaultValues() {
            var values = new Dictionary<string, string>();
            values[ShowMainButtos] = "save;print;cancel";
            return values;
        }
    }
}
