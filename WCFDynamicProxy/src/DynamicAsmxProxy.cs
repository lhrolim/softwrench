using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfSamples.DynamicProxy {
    class DynamicAsmxProxy :DynamicObject {
        
        public DynamicAsmxProxy(object obj) : base(obj)
        {
        }

        public DynamicAsmxProxy(Type objType) : base(objType)
        {
        }
    }
}
