using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.Association {
    public class DynamicOptionFieldResolver : BaseDependableResolver, ISingletonComponent {

        private const string WrongMethod = "Attribute provider {0} of dataset {1} was implemented with wrong signature. See IDataSet documentation";

        public IEnumerable<IAssociationOption> ResolveOptions(ApplicationSchemaDefinition schema, AttributeHolder originalEntity, OptionField optionField, SearchRequestDto associationFilter = null) {
            if (!FullSatisfied(optionField, originalEntity)) {
                return null;
            }
            var attribute = optionField.ProviderAttribute;
            var extraParameter = optionField.ExtraParameter;

            // transient field -> associations will be provided client-side
            if (attribute.StartsWith("#")) {
                return null;
            }

            var methodName = GetMethodName(attribute);
            var dataSet = FindDataSet(schema.ApplicationName, schema.SchemaId, methodName);
            var mi = dataSet.GetType().GetMethod(methodName);
            if (mi == null) {
                throw new InvalidOperationException(String.Format(MethodNotFound, methodName, dataSet.GetType().Name));
            }
            if (mi.GetParameters().Count() != 1 || mi.GetParameters()[0].ParameterType != typeof(OptionFieldProviderParameters)) {
                throw new InvalidOperationException(String.Format(WrongMethod, methodName, dataSet.GetType().Name));
            }
            var application = ApplicationMetadata.FromSchema(schema);
            var associationOptions = (IEnumerable<IAssociationOption>)mi.Invoke(dataSet, new object[] { new OptionFieldProviderParameters { OriginalEntity = originalEntity, ApplicationMetadata = application, OptionField = optionField } });
            if (associationOptions != null && optionField.Sort) {
                var enumerable = associationOptions as IAssociationOption[] ?? associationOptions.ToArray();
                if (enumerable.Any()) {
                    if (enumerable.First() is PriorityBasedAssociationOption) {
                        associationOptions = enumerable.OrderBy(f => ((PriorityBasedAssociationOption)f).Priority);
                    } else {
                        associationOptions = enumerable.OrderBy(f => f.Label);
                    }
                }
            }
            return associationOptions;
        }

        public static string GetMethodName(string attribute) {
            var internalAttribute = attribute.Replace("#", "");
            internalAttribute = internalAttribute.Replace("_", "");

            if (char.IsNumber(internalAttribute[0])) {
                //deprecated: using on the final of string to avoid angular errors
                return "Get" + StringUtil.FirstLetterToUpper(internalAttribute.Substring(1));
            }
            if (char.IsNumber(internalAttribute[internalAttribute.Length - 1])) {
                return "Get" + StringUtil.FirstLetterToUpper(internalAttribute.Substring(0, internalAttribute.Length - 1));
            }
            return "Get" + StringUtil.FirstLetterToUpper(internalAttribute);
        }
    }
}
