using System;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.test.Util;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sW4.test.Data.Persistence.DataSet.Commons.Ticket {
    [TestClass]
    public class TicketStatusHandlerTest {

        [TestMethod]
        public void TestNotAddingCurrent() {
            var options = new HashSet<IAssociationOption>();


            foreach (var value in Enum.GetValues(typeof(MaxSrStatus))) {
                options.Add(new AssociationOption(value.ToString(), value.ToString()));
            }

            var handler = new TicketStatusHandler(new TestContextLookuper());
            var datamap = DataMap.BlankInstance("sr").PopulateFromString("originalstatus:INPROG");
            datamap.SetAttribute("addcurrent", "False");

            var pparams = new AssociationPostFilterFunctionParameters {
                Options = options,
                OriginalEntity = datamap
            };

            var result = handler.FilterAvailableStatus(pparams);
            Assert.IsFalse(result.Any(a => a.Value.Equals(MaxSrStatus.INPROG.ToString())));


            datamap.SetAttribute("addcurrent", false);

            pparams = new AssociationPostFilterFunctionParameters {
                Options = options,
                OriginalEntity = datamap
            };

            result = handler.FilterAvailableStatus(pparams);
            Assert.IsFalse(result.Any(a => a.Value.Equals(MaxSrStatus.INPROG.ToString())));


            datamap.Remove("addcurrent");

            pparams = new AssociationPostFilterFunctionParameters {
                Options = options,
                OriginalEntity = datamap
            };

            result = handler.FilterAvailableStatus(pparams);
            Assert.IsTrue(result.Any(a => a.Value.Equals(MaxSrStatus.INPROG.ToString())));

        }

    }
}
