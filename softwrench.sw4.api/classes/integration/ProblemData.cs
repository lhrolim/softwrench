namespace softwrench.sw4.api.classes.integration {
    public class OperationProblemData {

        /// <summary>
        /// If not null would indicate that a problem with the given key needs to be opened upon Web-Service failure rather than simply returning an exception
        /// </summary>
        public string ProblemKey {
            get; set;
        }

        public OperationProblemData(string problemKey, bool propagateException=false) {
            ProblemKey = problemKey;
            PropagateException = propagateException;
        }

        public bool PropagateException {
            get; set;
        }

    }
}
