using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class PrefilterInvoker {

        protected const string MethodNotFound = "filterFunction {0} not found on DataSet {1}";

        private const string WrongPreFilterMethod = "PrefilterFunction {0} of dataset {1} was implemented with wrong signature. See IDataSet documentation";

        public static SearchRequestDto ApplyPreFilterFunction<T>(IDataSet dataSet,BasePreFilterParameters<T> preFilterParam, string prefilterFunctionName) {
            var mi = dataSet.GetType().GetMethod(prefilterFunctionName);
            if (mi == null) {
                throw new InvalidOperationException(String.Format(MethodNotFound, prefilterFunctionName, dataSet.GetType().Name));
            }
            if (mi.GetParameters().Count() != 1 || mi.ReturnType != typeof(SearchRequestDto) || mi.GetParameters()[0].ParameterType != typeof(AssociationPreFilterFunctionParameters)) {
                throw new InvalidOperationException(String.Format(WrongPreFilterMethod, prefilterFunctionName, dataSet.GetType().Name));
            }
            return (SearchRequestDto)mi.Invoke(dataSet, new object[] { preFilterParam });
        }

    }
}
