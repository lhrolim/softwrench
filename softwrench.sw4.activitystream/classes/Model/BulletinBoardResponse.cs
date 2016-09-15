using System.Collections.Generic;

namespace softwrench.sw4.activitystream.classes.Model {
    public class BulletinBoardResponse {
        public List<BulletinBoard> Messages {
            get; set;
        }

        public BulletinBoardResponse(List<BulletinBoard> messages) {
            Messages = messages;
        }
    }
}