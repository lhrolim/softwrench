using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Quartz.Util;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.SPF;

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
