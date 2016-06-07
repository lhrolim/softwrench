using cts.commons.simpleinjector;

namespace softwrench.sw4.problem.classes {
    public interface IProblemHandler  :IComponent{

        void OnProblemRegister(Problem problem);

        void OnProblemSolved();

        bool DelegateToMainApplication();

        string ProblemHandler();
        string ApplicationName();

        string ClientName();

    }
}