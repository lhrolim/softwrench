﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class DisplayableComponent {
        public String Id { get; set; }

        public IEnumerable<IApplicationDisplayable> RealDisplayables { get; set; }


    }
}
