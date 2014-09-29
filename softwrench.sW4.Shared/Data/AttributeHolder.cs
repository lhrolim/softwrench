using System.Collections.Generic;

namespace softwrench.sW4.Shared.Data {
    public abstract class AttributeHolder {
        public IDictionary<string, object> Attributes { get; set; }

        protected AttributeHolder() {

        }

        protected AttributeHolder(IDictionary<string, object> attributes) {
            Attributes = attributes;
        }

        ////        [JsonIgnore]
        //        public IDictionary<string, object> Attributes {
        //            get { return _attributes; }
        //        }

        public virtual object GetAttribute(string attributeName, bool remove = false) {
            object result;
            if (!Attributes.TryGetValue(attributeName, out result)) {
                Attributes.TryGetValue(attributeName.ToUpper(), out result);
            }
            if (remove && result != null) {
                Attributes.Remove(attributeName);
            }
            return result;
        }


    }
}