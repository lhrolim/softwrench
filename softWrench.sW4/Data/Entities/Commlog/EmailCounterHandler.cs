using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Data.Entities.Commlog {

    /// <summary>
    /// This code will get executed once the application starts to get a count of how many emails this particular has on the maximo database.
    /// 
    /// If it´s a large dataset then we need to make sure that the multipleselect component uses remote fetch (typeahead) implementation, otherwise a local implmentation can be used (which is way faster and would be preferred)
    /// 
    /// </summary>
    public class EmailCounterHandler : ISWEventListener<ApplicationStartedEvent> {

        private readonly MaximoHibernateDAO _dao;
        private readonly IMemoryContextLookuper _contextLookuper;

        public EmailCounterHandler(MaximoHibernateDAO dao, IMemoryContextLookuper contextLookuper) {
            _dao = dao;
            _contextLookuper = contextLookuper;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {

            //            _dao.CountByNativeQuery("select count (*) from email");
            //
            //            throw new NotImplementedException();
        }
    }
}
