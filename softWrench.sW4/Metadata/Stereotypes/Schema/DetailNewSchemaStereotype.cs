using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    class DetailNewSchemaStereotype : ASchemaStereotype {

        private static DetailNewSchemaStereotype _instance;

        public static DetailNewSchemaStereotype GetInstance() {
            return _instance ?? (_instance = new DetailNewSchemaStereotype());
        }

        protected DetailNewSchemaStereotype() { }



        protected override IDictionary<string, string> DefaultValues() {
            return new Dictionary<string, string>(){
            };
        }
    }
}
