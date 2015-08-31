using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes.Component {
    class EmailComponentStereotype : AComponentStereotype {

        private static EmailComponentStereotype _instance;

        public static string NewItemValidation = "newItemValidation";

        public static EmailComponentStereotype GetInstance() {
            return _instance ?? (_instance = new EmailComponentStereotype());
        }

        private EmailComponentStereotype() { }

        protected override IDictionary<string, string> DefaultValues() {
            var values = new Dictionary<string, string>();
            values[NewItemValidation] = "emailService.validateEmailAddress";
            return values;
        }
    }
}
