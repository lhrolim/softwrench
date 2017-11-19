using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.problem.classes.api {
    public class StringProblemData : IProblemData {

        private readonly string _data;

        public StringProblemData(string data) {
            _data = data;
        }

        public string Serialize() {
            return _data;
        }
    }
}
