using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.activitystream.classes.Controller;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.activitystream.classes.Util;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sW4.test.ActivityStream {
    [TestClass]
    public class NotificationSecurityGroupHelperTest {

        private readonly IDictionary<string, InMemoryNotificationStream> _notificationStreams = new Dictionary<string, InMemoryNotificationStream>()
        {
            {"p1",new InMemoryNotificationStream()},
            {"p2",new InMemoryNotificationStream()},
            {"defaultstream",new InMemoryNotificationStream()},
        };


        /// <summary>
        /// Test case where user is not informing the current profile and we have some profiles that defaultstream to the defaultstream stream.
        /// 
        /// The only non-defaultstream profile should be used, and 3 available ones only:
        /// 
        /// The ones which has its own stream, plus a "defaultstream fake profile" which will in turn merge all activities
        /// 
        /// </summary>
        [TestMethod]
        public void UserNotInformingCase() {

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, new List<UserProfile>{
                UserProfile.TestInstance(1,"p1"),
                UserProfile.TestInstance(2,"p2"),
                UserProfile.TestInstance(3,"p3"),
            });
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 3);
            Assert.IsTrue(result.AvailableProfiles.Any(f=> f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsTrue(result.AvailableProfiles.Any(f=> f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, -1);
        }

        [TestMethod]
        public void UserInformingCase() {

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, 2, new List<UserProfile>{
                UserProfile.TestInstance(1,"p1"),
                UserProfile.TestInstance(2,"p2"),
                UserProfile.TestInstance(3,"p3"),
            });
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 3);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, 2);
        }

        [TestMethod]
        public void UserAskingFordefaultstream() {

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, -1, new List<UserProfile>{
                UserProfile.TestInstance(1,"p1"),
                UserProfile.TestInstance(2,"p2"),
                UserProfile.TestInstance(3,"p3"),
            });
            //p1 and defaultstream

            Assert.AreEqual(result.AvailableProfiles.Count, 3);
            Assert.AreEqual(result.AvailableProfiles.First().Name,"defaultstream");
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, -1);
        }

        //No defaultstream
        [TestMethod]
        public void UserWithAllExactProfiles() {

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, new List<UserProfile>{
                UserProfile.TestInstance(1,"p1"),
                UserProfile.TestInstance(2,"p2"),
            });
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 2);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsFalse(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, 1);
        }

        [TestMethod]
        public void UserWithJustOneProfile() {

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, new List<UserProfile>{
                UserProfile.TestInstance(10,"p1"),
            });
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.AreEqual(result.SelectedProfile.Id, 10);
        }

        [TestMethod]
        public void UserWithNoProfile() {

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, new List<UserProfile>());
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, -1);
        }
    }
}
