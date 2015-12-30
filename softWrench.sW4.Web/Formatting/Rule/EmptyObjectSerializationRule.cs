using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace softWrench.sW4.Web.Formatting.Rule {
    /// <summary>
    /// Prevents serialization of empty objects ("{}").
    /// </summary>
    public class EmptyObjectSerializationRule : IPropertySerializationRule {

        private static readonly IDictionary<Type, Predicate<object>> ProcessorCache = new Dictionary<Type, Predicate<object>>();

        public bool Matches(JsonProperty property, MemberInfo member, MemberSerialization memberSerialization) {
            return property.PropertyType.Namespace.ContainsIgnoreCase("softwrench.sw4.");
        }

        public bool ShouldSerialize(object instance, JsonProperty property, MemberInfo member, MemberSerialization memberSerialization, LeanJsonContractResolver contractResolver) {
            var target = property.ValueProvider.GetValue(instance);
            // property has null value -> shouldn't be serialized
            if (target == null) return false;

            var type = property.PropertyType;
            var childrenProperties = contractResolver.PropertiesForType(type, memberSerialization);
            // dto has no properties it doesn't need to be serialized
            if (childrenProperties.Count <= 0) {
                return false;
            }
            // if none of it's properties should be serialized then it also shouldn't be serialized (combining predicates with OR)
            Predicate<object> shouldSerialize;
            if (!ProcessorCache.TryGetValue(type, out shouldSerialize)) {
                shouldSerialize = PredicateUtil.Or(childrenProperties.Select(p => p.ShouldSerialize).ToArray());
                ProcessorCache[type] = shouldSerialize; // caching to avoid precomputing all the time
            }
            return shouldSerialize(target);
        }
    }
}
