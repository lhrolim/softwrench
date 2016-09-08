using System.Web.Http;
using cts.commons.persistence;
using cts.commons.web.Attributes;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.controller {

    [System.Web.Mvc.Authorize]
    [SWControllerConfiguration]
    public class ChicagoSrController : ApiController {

        private readonly IMaximoHibernateDAO _dao;

        public ChicagoSrController(IMaximoHibernateDAO dao) {
            _dao = dao;
        }

        /// <summary> 
        /// </summary>
        /// <param name="newOwnerGroup"></param>
        /// <param name="owner"></param>
        /// <returns>The list of locations of interest.</returns>
        [HttpGet]
        public bool IsOwnerMember([FromUri] string newOwnerGroup, [FromUri]string owner) {
            var result = _dao.FindByNativeQuery(
                "select 1 from person where personid = ? and personid in (select respparty from persongroupteam where persongroup= ?)", owner, newOwnerGroup);
            return result.Count > 0;

        }


    }
}
