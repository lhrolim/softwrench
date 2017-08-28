using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.connector {

    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FirstSolarAttachmentHandler : AttachmentHandler {
        public FirstSolarAttachmentHandler(MaximoHibernateDAO maxDAO, DataSetProvider dataSetProvider, AttachmentDao attachmentDao) : base(maxDAO, dataSetProvider, attachmentDao) {
        }

        protected override int GetMaximoLength() {
            return 20;
        }
    }
}
