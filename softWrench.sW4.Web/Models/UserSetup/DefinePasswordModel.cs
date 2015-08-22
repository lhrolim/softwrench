using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Models.UserSetup {
    public class DefinePasswordModel : IBaseLayoutModel {

        public DefinePasswordModel() {
            ClientName = ApplicationConfiguration.ClientName;
        }

        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string ClientName { get; set; }

        public string Title { get { return "Define Password"; } }
    }
}