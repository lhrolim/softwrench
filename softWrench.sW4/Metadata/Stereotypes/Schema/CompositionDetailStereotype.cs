using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    class CompositionDetailStereotype : ASchemaStereotype {

        private static CompositionDetailStereotype _instance;

        public static CompositionDetailStereotype GetInstance() {
            return _instance ?? (_instance = new CompositionDetailStereotype());
        }

        private CompositionDetailStereotype() { }


        protected override IDictionary<string, string> DefaultValues() {
            return new Dictionary<string, string>();
        }
    }
}
