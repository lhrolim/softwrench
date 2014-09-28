using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.Mobile.Communication.Synchronization {
    class SynchronizationResult {

        private readonly int _successes;
        private readonly int _errors;

        public SynchronizationResult(int successes, int errors) {
            _successes = successes;
            _errors = errors;
        }

        public int Successes {
            get { return _successes; }
        }

        public int Errors {
            get { return _errors; }
        }
    }
}
