using System;
using softwrench.sw4.api.classes.integration;

namespace softwrench.sw4.batch.api {
    public class BatchOptions {

        private bool _transient;

        public bool Synchronous {
            get; set;
        }

        /// <summary>
        /// Legacy: use ProblemKey instead
        /// </summary>
        public bool GenerateProblems {
            get; set;
        }

        public OperationProblemData ProblemData { get; set; }

        /// <summary>
        /// If present this would be the key used to open a problem.
        /// </summary>
        public string ProblemKey {
            get; set;
        }

        /// <summary>
        /// if present this will be used to match the converters and configurerers instead of the schemaId
        /// </summary>
        public string BatchOperationName { get; set; }

        /// <summary>
        /// Allows overriding the default config /Global/Batches/MaxThreads property
        /// </summary>
        public string MaxThreadsProperty {
            get; set;
        }

        public bool GenerateReport {
            get; set;
        }

        public bool Transient {
            get { return _transient; }
            set {
                _transient = value;
                GenerateReport = !value;
            }
        }

        public bool SendEmail {
            get; set;
        }




    }
}
