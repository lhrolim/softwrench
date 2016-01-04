using System.Reflection;
using cts.commons.simpleinjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace softWrench.sW4.Web.Formatting.Rule {
    public interface IPropertySerializationRule : IComponent {

        /// <summary>
        /// Indicates whether or not this rule should be applied for the property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        bool Matches(JsonProperty property, MemberInfo member, MemberSerialization memberSerialization);

        /// <summary>
        /// Application of the rule: decides whether or not the property of the object instance should be serialized or not.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="property"></param>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <param name="contractResolver"></param>
        /// <returns></returns>
        bool ShouldSerialize(object instance, JsonProperty property, MemberInfo member, MemberSerialization memberSerialization, LeanJsonContractResolver contractResolver);

    }
}
