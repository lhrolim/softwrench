using cts.commons.simpleinjector;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.problem.classes {
    public interface IProblemHandler  :IComponent{

        void OnProblemRegister(Problem problem);

        void OnProblemSolved();

        bool DelegateToMainApplication();

        string ProblemHandler();
        string ApplicationName();

        string ClientName();

        ApplicationMetadataSchemaKey OnLoad(AttributeHolder resultObject, string data);
    }
}