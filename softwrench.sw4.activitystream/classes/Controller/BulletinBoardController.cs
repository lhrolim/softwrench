using System.Net.Http;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.activitystream.classes.Model;

namespace softwrench.sw4.activitystream.classes.Controller {
    [Authorize]
    [SWControllerConfiguration]
    public class BulletinBoardController : ApiController {

        private readonly BulletinBoardFacade _bulletinBoardFacade;

        public BulletinBoardController(BulletinBoardFacade bulletinBoardFacade) {
            _bulletinBoardFacade = bulletinBoardFacade;
        }

        /// <summary>
        /// BulletinBoard SSE subscribe endpoint.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Subscribe(HttpRequestMessage request) {
            var response = _bulletinBoardFacade.AddBulletinBoardUpdateSubscriber(request);
            return response;
        }

        [HttpGet]
        public BulletinBoardResponse ActiveMessages() {
            return _bulletinBoardFacade.GetActiveBulletinBoardsState();
        }
    }
}