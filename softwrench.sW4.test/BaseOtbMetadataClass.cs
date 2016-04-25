﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test {
    public abstract class BaseOtbMetadataClass : BaseMetadataTest {

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != GetClientName()) {
                ApplicationConfiguration.TestclientName = GetClientName();
                MetadataProvider.StubReset();
            }
        }

        public virtual string GetClientName() {
            return "otb";
        }

    }
}