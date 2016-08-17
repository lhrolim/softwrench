using System.Collections.Generic;
using System.Linq;
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

        [HttpGet]
        public BulletinBoardResponse GetActiveMessages() {
            var messages = _bulletinBoardFacade.GetActiveBulletinBoards();
            return new BulletinBoardResponse(messages.ToList(), _bulletinBoardFacade.BulletinBoardUiRefreshRate);
        }

        public class BulletinBoardResponse {
            public List<BulletinBoard> Messages { get; set; }
            public long RefreshRate { get; set; }

            public BulletinBoardResponse(List<BulletinBoard> messages, long refreshRate) {
                Messages = messages;
                RefreshRate = refreshRate;
            }
        }
    }
}