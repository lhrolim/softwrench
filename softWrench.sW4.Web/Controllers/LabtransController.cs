using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Linq;
using NHibernate.Util;
using Quartz.Util;
using cts.commons.web.Attributes;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.Controllers {
    public class LabtransController : ApiController {
        private readonly MaximoHibernateDAO _maximoDao;

        public LabtransController(MaximoHibernateDAO dao) {
            _maximoDao = dao;
        }

        [HttpPost]
        public IGenericResponseResult ApproveLabtrans([FromBody]List<string> labtransIds) {
            if (labtransIds == null || !labtransIds.Any()) {
                return null;
            }
            var lbtransIdstring = string.Join(", ", labtransIds);
            
            // TODO: For now we will be updating the records directly until we can figure out a way to do it through the Maximo WS
            _maximoDao.ExecuteSql("update labtrans set genapprservreceipt = '1' where labtransid in ({0}) ".FormatInvariant(lbtransIdstring));

            return new GenericResponseResult<WebResponse>();
        }

        [HttpPost]
        public IGenericResponseResult DeleteLabtrans([FromBody]List<string> labtransIds) {
            if (labtransIds == null || !labtransIds.Any()) {
                return null;
            }
            foreach (var labtransId in labtransIds) {
                // TODO: Like the labtranshandler, for now we will be deleting the records directly until we can figure out a way to do it through the Maximo WS
                _maximoDao.ExecuteSql("delete from labtrans where labtransid = ? ", labtransId);
            }

            return new GenericResponseResult<WebResponse>();
        }
    }
}
