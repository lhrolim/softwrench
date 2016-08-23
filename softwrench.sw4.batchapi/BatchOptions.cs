using System;

namespace softwrench.sw4.batch.api {
    public class BatchOptions {


        public bool Synchronous {
            get; set;
        }

        /// <summary>
        /// Legacy: use ProblemKey instead
        /// </summary>
        public bool GenerateProblems {
            get; set;
        }

        /// <summary>
        /// If present this would be the key used to open a problem.
        /// </summary>
        public string ProblemKey {
            get; set;
        }

        /// <summary>
        /// Allows overriding the default config /Global/Batches/MaxThreads property
        /// </summary>
        public string MaxThreadsProperty {
            get; set;
        }

        public bool GenerateReport {
            get; set;
        }

        public bool SendEmail {
            get; set;
        }




    }
}
