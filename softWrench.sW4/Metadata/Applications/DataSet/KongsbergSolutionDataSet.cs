using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using System.Collections.Generic;
using softWrench.sW4.Metadata.Applications.Classification;

namespace softWrench.sW4.Metadata.Applications.DataSet {
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
