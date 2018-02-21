using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Util;

namespace softwrench.sw4.webcommons.classes.api {
    public class ABaseLayoutModel : IBaseLayoutModel {

        public virtual string Title { get; set; }
        public virtual string ClientName { get { return ApplicationConfiguration.ClientName; } set { } }
        public virtual bool PreventPoweredBy => ClientName.Equals("firstsolardispatch");
        public virtual string H1Header { get; set; }
        public virtual string H1HeaderStyle { get; set; }
    }
}
