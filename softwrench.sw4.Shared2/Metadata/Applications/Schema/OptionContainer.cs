using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Data.Association;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class OptionContainer {

        public string Id {
            get; set;
        }
        public IEnumerable<IAssociationOption> Options {
            get; set;
        }

        public OptionContainer(string id, IEnumerable<IAssociationOption> options) {
            Id = id;
            Options = options;
        }

    }
}
