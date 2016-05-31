using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Data {


    public abstract class AttributeHolder {
        private const string NotFound = "attribute {0} not found in {1}. Please review your metadata configuration";

        //TODO: make Attributes a generic type instead of object
        [JsonIgnore]
        public IDictionary<string, object> Attributes {
            get; set;
        }

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

        public virtual string GetStringAttribute(string attributeName, bool remove = false, bool throwException = false) {
            var attribute = GetAttribute(attributeName, remove, throwException);
            if (attribute != null) {
                return attribute.ToString();
            }
            return null;
        }

        public void SetAttribute(string attributeName, object value) {
            if (Attributes.ContainsKey(attributeName)) {
                Attributes.Remove(attributeName);
            }
            Attributes.Add(attributeName, value);
        }

        public virtual bool ContainsAttribute(string attributeName, bool checksForNonNull = false) {
            return Attributes.ContainsKey(attributeName);
        }

        public abstract string HolderName();

        public static TestAttributeHolder TestInstance(IDictionary<string, object> attributes, string holderName = "") {
            return new TestAttributeHolder(attributes, holderName);
        }

        public class TestAttributeHolder : AttributeHolder {
            private readonly string _holderName;

            public TestAttributeHolder(IDictionary<string, object> attributes, string holderName) : base(attributes) {
                _holderName = holderName;
            }

            public override string HolderName() {
                return _holderName;
            }
        }
    }
}