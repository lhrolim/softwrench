using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Models.UserSetup {
    public class DefinePasswordModel : IBaseLayoutModel {

        public DefinePasswordModel() {
            ClientName = ApplicationConfiguration.ClientName;
        }

        public string Token { get; set; }

        public bool ChangePasswordScenario { get; set; }

        /// <summary>
        /// This password doesn´t match the required policies of the system
        /// </summary>
        public bool InvalidPassword { get; set; }

        /// <summary>
        /// This password although valid cannot be choosen cause it was used recently. 
        /// </summary>
        public bool RepeatedPassword {get; set;}

        public string FullName { get; set; }

        public string Username { get; set; }

        public string ClientName { get; set; }

        public string Title { get; set; }
    }
}