using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace softWrench.sW4.Web.Formatting.Rule {
    /// <summary>
    /// Skip serialization of all empty collections that ARE NOT IDictionary (all types implementing ICollection and having Count == 0), 
    /// unless DefaultValueHandling.Include is specified for the property or the field.
    /// source: http://stackoverflow.com/questions/18471864/how-to-define-in-json-net-a-default-value-to-an-empty-list-or-dictionary
    /// not ignoring empty dictionaries because that will lead to multiple null checks in the client-side
    /// </summary>
    public class EmptyCollectionSerializationRule : IPropertySerializationRule {
        public bool Matches(JsonProperty property, MemberInfo member, MemberSerialization memberSerialization) {
            var propertyType = property.PropertyType;
            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(propertyType) || (propertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()));
            var isDictionary = typeof(IDictionary).IsAssignableFrom(propertyType) || (propertyType.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()));
            return isEnumerable && !isDictionary;
        }

        public bool ShouldSerialize(object instance, JsonProperty property, MemberInfo member, MemberSerialization memberSerialization, LeanJsonContractResolver contractResolver) {
            var collection = property.ValueProvider.GetValue(instance) as ICollection;
            return collection != null && collection.Count > 0;
        }
    }
}