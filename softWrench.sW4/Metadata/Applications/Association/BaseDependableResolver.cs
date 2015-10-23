using System;
using System.Linq;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softWrench.sW4.Metadata.Applications.Association {
    public abstract class BaseDependableResolver {

        protected const string MethodNotFound = "filterFunction {0} not found on DataSet {1}";
        protected const string DataSetNotFound = "Application {0} requires a filterFunction {1}, but no DataSet could be located";

        protected bool FullSatisfied(IDependableField dependableField, AttributeHolder originalEntity) {
            var dependantFields = dependableField.DependantFields;
            if (dependantFields.Count == 0) {
                return true;
            }
            return dependantFields.All(depField => originalEntity.GetAttribute(depField) != null);
        }

        protected IDataSet FindDataSet(String applicationName, string schemaId, string missingParameter) {
            var dataSet = DataSetProvider.GetInstance().LookupDataSet(applicationName, schemaId);
            if (dataSet == null) {
                throw new InvalidOperationException(String.Format(DataSetNotFound, applicationName,
                                                                  missingParameter));
            }
            return dataSet;
        }

    }
}
