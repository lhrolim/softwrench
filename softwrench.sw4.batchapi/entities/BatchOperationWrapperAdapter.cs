using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.batch.api.entities {

    public class TransientBatchOperationData {

        public IEnumerable<DataMapDefinition> Datamaps { get; set; }

        [CanBeNull]
        public string OperationName {
            get; set;
        }

        public IEnumerable<IOperationWrapper> OperationWrappers {
            get; set;
        }

        [CanBeNull]
        public Func<IOperationWrapper, bool> BeforeWSExecution { get; set; }

        public ApplicationMetadata AppMetadata { get; set; }


        public BatchOptions BatchOptions { get; set; }



    }
}
