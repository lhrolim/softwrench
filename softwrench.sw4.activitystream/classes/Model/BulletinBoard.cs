using System;
using System.Collections.Generic;

namespace softwrench.sw4.activitystream.classes.Model {
    public class BulletinBoard {
        public long BulletinBoardUid { get; set; }
        public string BulletinBoardId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string PostBy { get; set; }
        public string Status { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime PostDate { get; set; }

        public static BulletinBoard FromDictionary(IDictionary<string, object> dict) {
            return new BulletinBoard() {
                BulletinBoardUid = (long)dict["bulletinboarduid"],
                BulletinBoardId = dict["bulletinboardid"].ToString(),
                Subject = dict["subject"].ToString(),
                Message = dict["message"].ToString(),
                PostBy = dict["postby"].ToString(),
                Status = dict["status"].ToString(),
                ExpireDate = (DateTime)dict["expiredate"],
                PostDate = (DateTime)dict["postdate"],
            };
        }

    }
}
