using System;

namespace softwrench.sw4.api.classes.fwk.dataset {

    [System.AttributeUsage(AttributeTargets.Method)]
    public class PreFilterAttribute : Attribute {

        public string RelationshipName {
            get; set;
        }

        public PreFilterAttribute(string relationshipName) {
            this.RelationshipName = relationshipName;
        }



    }
}
