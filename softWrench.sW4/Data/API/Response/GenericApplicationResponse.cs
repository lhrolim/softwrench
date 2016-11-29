using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.API.Response {
    public class GenericApplicationResponse : BlankApplicationResponse {
        public object ResultObject { get; set; }

        public new bool FullRefresh { get; set; } = false;


        public override string Type => typeof(GenericApplicationResponse).Name;
    }
}
