using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softwrench.sw4.activitystream.classes.Controller;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.activitystream.classes.Util {
    public class NotificationSecurityGroupHelper {
        private const int FakeDefaultProfileId = -1;

        [NotNull]
        public static NotificationSecurityGroupDTO GetNotificationProfile(IDictionary<string, InMemoryNotificationStream> notificationStreams, int? clientSelectedProfile, ICollection<UserProfile> profiles) {
            var result = new NotificationSecurityGroupDTO();
            var availableProfiles = profiles.Where(p => notificationStreams.ContainsKey(p.Name)).Select(s => s.ToDTO());
            var userProfileDtos = availableProfiles as UserProfile.UserProfileDTO[] ?? availableProfiles.ToArray();
            var deafaultProfile = new UserProfile.UserProfileDTO(FakeDefaultProfileId, ActivityStreamConstants.DefaultStreamName);
            if (userProfileDtos.Length != profiles.Count || !profiles.Any()) {
                //this means that some profiles use the default notification, so let´s add a default one which will represent the merge of any groups which don´t have a specific stream
                result.AvailableProfiles.Add(deafaultProfile);
            }
            result.AvailableProfiles.AddRange(userProfileDtos);
            if (clientSelectedProfile != null) {
                result.SelectedProfile = result.AvailableProfiles.First(p => p.Id == clientSelectedProfile);
            } else {
                //pick default
                if (result.AvailableProfiles.Contains(deafaultProfile)) {
                    result.SelectedProfile = deafaultProfile;
                } else {
                    result.SelectedProfile = result.AvailableProfiles.First();
                }
            }
            return result;
        }

        public static string GetApplicationNameByRole(string key) {
            //TODO: adjust role names to match application names, or create a external translator
            if (key.Equals("sr") || key.Equals("ssr")) {
                return "servicerequest";
            }
            if (key.Equals("workorders")) {
                return "workorder";
            }
            return key;

        }

        public class NotificationSecurityGroupDTO {


            [NotNull]
            public List<UserProfile.UserProfileDTO> AvailableProfiles = new List<UserProfile.UserProfileDTO>();

            [NotNull]
            public UserProfile.UserProfileDTO SelectedProfile {
                get; set;
            }
        }

        public static string GetGroupNameById(int? securityGroup, InMemoryUser currentUser) {
            if (securityGroup == -1) {
                return ActivityStreamConstants.DefaultStreamName;
            }
            return currentUser.Profiles.First(p => p.Id == securityGroup).Name;
        }
    }
}
