using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {
    public enum SchemaMode {
        //keep it as lowercase to avoid bugs on the client side.
        input,output,
        None
    }
}
