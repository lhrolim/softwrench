using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping;
using softWrench.sW4.Data;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Metadata.Applications.Association {
    class DynamicOptionFieldResolver : BaseDependableResolver, ISingletonComponent {

        private const string WrongMethod = "Attribute provider {0} of dataset {1} was implemented with wrong signature. See IDataSet documentation";

        public IEnumerable<IAssociationOption> ResolveOptions(ApplicationMetadata application, OptionField optionField, AttributeHolder dataMap) {
            if (!FullSatisfied(optionField, dataMap)) {
                return null;
            }
            if (optionField.ShowExpression == "false") {
                return null;
            }

            var attribute = optionField.ProviderAttribute;
            attribute = attribute.Replace("#", "");
            attribute = attribute.Replace("_", "");


            var methodName = GetMethodName(attribute);
            var dataSet = FindDataSet(application.Name, methodName);
            var mi = dataSet.GetType().GetMethod(methodName);
            if (mi == null) {
                throw new InvalidOperationException(String.Format(MethodNotFound, methodName, dataSet.GetType().Name));
            }
            if (mi.GetParameters().Count() != 1 || mi.GetParameters()[0].ParameterType != typeof(OptionFieldProviderParameters)) {
                throw new InvalidOperationException(String.Format(WrongMethod, methodName, dataSet.GetType().Name));
            }
            var associationOptions = (IEnumerable<IAssociationOption>)mi.Invoke(dataSet, new object[] { new OptionFieldProviderParameters { OriginalEntity = dataMap, ApplicationMetadata = application, OptionField = optionField } });
            if (optionField.Sort) {
                associationOptions = associationOptions.OrderBy(f => f.Label);
            }
            return associationOptions;
        }

        private static string GetMethodName(string attribute) {
            if (Char.IsNumber(attribute[0])) {
                return "Get" + StringUtil.FirstLetterToUpper(attribute.Substring(1));
            }
            return "Get" + StringUtil.FirstLetterToUpper(attribute);
        }
    }
}
