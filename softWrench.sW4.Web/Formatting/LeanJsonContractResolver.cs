using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Web.Formatting.Rule;

namespace softWrench.sW4.Web.Formatting {
    /// <summary>
    /// Contract resolver for serializing domain objects to lean json documents.
    /// It is a subclass of <see cref="CamelCasePropertyNamesContractResolver"/>
    /// </summary>
    public class LeanJsonContractResolver : CamelCasePropertyNamesContractResolver {

        private static readonly IDictionary<Type, IList<JsonProperty>> PropertyCache = new Dictionary<Type, IList<JsonProperty>>();

        private IList<IPropertySerializationRule> _rules;

        private IList<IPropertySerializationRule> Rules {
            get {
                return _rules ?? (_rules = SimpleInjectorGenericFactory.Instance
                    .GetObjectsOfType<IPropertySerializationRule>(typeof(IPropertySerializationRule))
                    .ToList());
            }
        }

        public IList<JsonProperty> PropertiesForType(Type type, MemberSerialization memberSerialization) {
            IList<JsonProperty> properties;
            if (!PropertyCache.TryGetValue(type, out properties)) {
                properties = CreateProperties(type, memberSerialization);
                PropertyCache[type] = properties;
            }
            return properties;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            var properties = base.CreateProperties(type, memberSerialization);
            PropertyCache[type] = properties;
            return properties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);
            var customRule = CombinedRules(property, member, memberSerialization);
            // property type does not require custom serialization rule
            if (customRule == null) return property;
            // apply custom serialization rule
            var oldRule = property.ShouldSerialize;
            property.ShouldSerialize = oldRule != null // already has predicate ?
                ? o => oldRule(o) && customRule(o) // combining already existing predicate with new one
                : customRule; // using just the new predicate
            return property;
        }

        /// <summary>
        /// Combines the serialization rules that apply to the given context into a single predicate with the 'and' operator:
        /// obj => rule1.should(obj, ...) && rule2.should(obj, ...) ... rulen.should(obj, ...);
        /// If no rules apply will return a null predicate.
        /// source: http://stackoverflow.com/questions/1248232/combine-multiple-predicates
        /// </summary>
        /// <param name="property"></param>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        private Predicate<object> CombinedRules(JsonProperty property, MemberInfo member, MemberSerialization memberSerialization) {
            var rulesToApply = Rules.Where(r => r.Matches(property, member, memberSerialization)).ToList();
            if (rulesToApply.Count <= 0) return null;
            return delegate (object obj) {
                foreach (var rule in rulesToApply) {
                    if (!rule.ShouldSerialize(obj, property, member, memberSerialization, this)) {
                        return false;
                    }
                }
                return true;
            };
        }
    }
}