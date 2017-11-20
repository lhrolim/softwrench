using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.problem.classes.api {

    public class ProblemExceptionWrapper : Exception {

        private readonly Problem _problem;

        public ProblemExceptionWrapper(Exception original, Problem problem) : base(original.Message, original.InnerException) {
            _problem = problem;
        }

        public Problem Problem {
            get { return _problem; }
        }
    }
}
