using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.api.classes.integration
{
    public interface ICommonOperationData
    {
        string Id {
            get; set;
        }

        string UserId {
            get; set;
        }

        string Class {
            get;
        }

        ApplicationMetadata ApplicationMetadata {
            get; set;
        }

   

        OperationProblemData ProblemData {
            get; set;
        }

        AttributeHolder Holder { get; }
    }
}