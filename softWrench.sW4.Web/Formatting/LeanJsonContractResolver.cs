using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace softWrench.sW4.Web.Formatting {
    /// <summary>
    /// Contract resolver for serializing domain objects to lean json documents.
    /// It is a subclass of <see cref="CamelCasePropertyNamesContractResolver"/>
    /// </summary>
    public class LeanJsonContractResolver : CamelCasePropertyNamesContractResolver {

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);
            var customRule = CustomSerializationRuleForProperty(property, member, memberSerialization);
            // property type does not require custom serialization rule
            if (customRule == null) return property;
            // apply custom serialization rule
            var oldRule = property.ShouldSerialize;
            property.ShouldSerialize = oldRule != null // already has predicate ?
                ? o => oldRule(o) && customRule(o) // combining already existing predicate with new one
                : customRule; // using just the new predicate
            return property;
        }

        private static Predicate<object> CustomSerializationRuleForProperty(JsonProperty property, MemberInfo member, MemberSerialization memberSerialization) {
            var isDefaultValueIgnored = ((property.DefaultValueHandling ?? DefaultValueHandling.Ignore) & DefaultValueHandling.Ignore) != 0;
            var propertyType = property.PropertyType;
            var isInterceptable = isDefaultValueIgnored && !typeof(string).IsAssignableFrom(propertyType);
            if (!isInterceptable) return null;
            
            // skip serialization of all empty collections that ARE NOT IDictionary (all types implementing ICollection and having Count == 0), 
            // unless DefaultValueHandling.Include is specified for the property or the field.
            // source: http://stackoverflow.com/questions/18471864/how-to-define-in-json-net-a-default-value-to-an-empty-list-or-dictionary
            // not ignoring empty dictionaries because that will lead to multiple null checks in the client-side
            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(propertyType) || (propertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()));
            var isDictionary = typeof(IDictionary).IsAssignableFrom(propertyType) || (propertyType.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()));
            if (isEnumerable && !isDictionary) {
                return obj => {
                    var collection = property.ValueProvider.GetValue(obj) as ICollection;
                    return collection == null || collection.Count > 0;
                };
            }
            

            // else if (<need some other custom serialization rule>) { ... return <rule as predicate> }
             
            return null;
        }
    }
}