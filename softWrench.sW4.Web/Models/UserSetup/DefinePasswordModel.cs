using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Models.UserSetup {
    public class DefinePasswordModel : IBaseLayoutModel {

        public DefinePasswordModel() {
            ClientName = ApplicationConfiguration.ClientName;
        }

        public string Token { get; set; }

        public bool ChangePasswordScenario { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public string ClientName { get; set; }

        public string Title { get; set; }
    }
}