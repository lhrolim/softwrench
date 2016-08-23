﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Data {


    public abstract class AttributeHolder : Dictionary<string, object> {
        private const string NotFound = "attribute {0} not found in {1}. Please review your metadata configuration";

        protected AttributeHolder() : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected AttributeHolder(IDictionary<string, object> attributes) : base(StringComparer.OrdinalIgnoreCase) {
            AddAllAttribtes(attributes);
        }

        public virtual void AddAllAttribtes(IDictionary<string, object> fields) {
            foreach (KeyValuePair<string, object> entry in fields) {
                Add(entry.Key, entry.Value);
            }
        }

        public virtual object GetAttribute(string attributeName, bool remove = false, bool throwException = false) {
            object result;
            if (!TryGetValue(attributeName, out result)) {
                if (!TryGetValue(attributeName.ToUpper(), out result)) {
                    if (!TryGetValue(attributeName.ToLower(), out result) && throwException) {
                        throw new InvalidOperationException(String.Format(NotFound, attributeName, HolderName()));
                    }


                }
            }
            if (remove && result != null) {
                Remove(attributeName);
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
            if (ContainsKey(attributeName)) {
                Remove(attributeName);
            }
            Add(attributeName, value);
        }

        public virtual bool ContainsAttribute(string attributeName, bool checksForNonNull = false) {
            return ContainsKey(attributeName);
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