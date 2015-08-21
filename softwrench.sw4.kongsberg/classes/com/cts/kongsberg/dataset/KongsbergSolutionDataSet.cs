using System.Collections.Generic;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.Classification;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    class KongsbergSolutionDataSet : MaximoApplicationDataSet {

        public IEnumerable<IAssociationOption> GetSlnClassStructureType(OptionFieldProviderParameters parameters) {
            return ClassificationDataSet.GetClassStructureType(ClassificationDataSet.ClassStructureType.Solution,
                parameters);
        }

        public override string ApplicationName() {
            return "solution";
        }

        public override string ClientFilter() {
            return "kongsberg";
        }
    }
}
