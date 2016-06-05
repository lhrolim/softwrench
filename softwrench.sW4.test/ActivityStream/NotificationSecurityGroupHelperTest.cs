using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.activitystream.classes.Util;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata.Security;

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
            var user = CreateUser(true, false, false);
            user.Profiles.Add(UserProfile.TestInstance(1, "p1"));
            user.Profiles.Add(UserProfile.TestInstance(2, "p2"));
            user.Profiles.Add(UserProfile.TestInstance(3, "p3"));

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 3);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, -1);
        }

        [TestMethod]
        public void UserInformingCase() {
            var user = CreateUser(true, false, false);
            user.Profiles.Add(UserProfile.TestInstance(1, "p1"));
            user.Profiles.Add(UserProfile.TestInstance(2, "p2"));
            user.Profiles.Add(UserProfile.TestInstance(3, "p3"));

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, 2, user);
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 3);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, 2);
        }

        [TestMethod]
        public void UserAskingFordefaultstream() {
            var user = CreateUser(true, false, false);
            user.Profiles.Add(UserProfile.TestInstance(1, "p1"));
            user.Profiles.Add(UserProfile.TestInstance(2, "p2"));
            user.Profiles.Add(UserProfile.TestInstance(3, "p3"));

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, -1, user);
            //p1 and defaultstream

            Assert.AreEqual(result.AvailableProfiles.Count, 3);
            Assert.AreEqual(result.AvailableProfiles.First().Name, "defaultstream");
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, -1);
        }

        //No defaultstream
        [TestMethod]
        public void UserWithAllExactProfiles() {
            var user = CreateUser(true, false, false);
            user.Profiles.Add(UserProfile.TestInstance(1, "p1"));
            user.Profiles.Add(UserProfile.TestInstance(2, "p2"));

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 2);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p2"));
            Assert.IsFalse(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, 1);
        }

        [TestMethod]
        public void UserWithJustOneProfile() {
            var user = CreateUser(true, false, false);
            user.Profiles.Add(UserProfile.TestInstance(10, "p1"));

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "p1"));
            Assert.AreEqual(result.SelectedProfile.Id, 10);
        }

        [TestMethod]
        public void UserWithNoProfile() {
            var user = CreateUser(true, false, false);

            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //p1 and defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
            Assert.AreEqual(result.SelectedProfile.Id, -1);
        }

        [TestMethod]
        public void IsSwAdmin() {
            var user = CreateUser(true, false, false);
            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
        }

        [TestMethod]
        public void HasAdminRole() {
            var user = CreateUser(false, true, false);
            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
        }

        [TestMethod]
        public void HasNotificationRole() {
            var user = CreateUser(false, false, true);
            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            //defaultstream
            Assert.AreEqual(result.AvailableProfiles.Count, 1);
            Assert.IsTrue(result.AvailableProfiles.Any(f => f.Name == "defaultstream"));
        }

        [TestMethod]
        public void HasNoRole() {
            var user = CreateUser(false, false, false);
            var result = NotificationSecurityGroupHelper.GetNotificationProfile(_notificationStreams, null, user);
            Assert.AreEqual(result.AvailableProfiles.Count, 0);
        }

        private static InMemoryUser CreateUser(bool isSwAdmin, bool isSysAdmin, bool hasNotificationRole) {
            var user = InMemoryUser.TestInstance(isSwAdmin ? "swadmin" : "isnotadmin");
            if (isSysAdmin) {
                user.Roles.Add(new Role() {
                    Name = Role.SysAdmin
                });
            }
            if (hasNotificationRole) {
                user.Roles.Add(new Role() {
                    Name = NotificationsRolesManager.NotificationsRole
                });
            }
            return user;
        }
    }
}
