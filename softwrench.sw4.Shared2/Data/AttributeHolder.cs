using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Data {
    public abstract class AttributeHolder {
        private const string NotFound = "attribute {0} not found in {1}. Please review your metadata configuration";

        [JsonIgnore]
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

        public virtual object GetAttribute(string attributeName, bool remove = false, bool throwException = false) {
            object result;
            if (!Attributes.TryGetValue(attributeName, out result)) {
                if (!Attributes.TryGetValue(attributeName.ToUpper(), out result) && throwException) {
                    throw new InvalidOperationException(String.Format(NotFound, attributeName, HolderName()));
                }
            }
            if (remove && result != null) {
                Attributes.Remove(attributeName);
            }
            

            return result;
        }

        public void SetAttribute(string attributeName, object value) {
            if (Attributes.ContainsKey(attributeName)) {
                Attributes.Remove(attributeName);
            }
            Attributes.Add(attributeName, value);
        }

        public virtual bool ContainsAttribute(string attributeName) {
            return Attributes.ContainsKey(attributeName);
        }

        protected abstract string HolderName();


    }
}