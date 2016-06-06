using System;
using System.IO;

namespace softWrench.sW4.Web.Util {
    public class CustomerResourceResolver {

        /// <summary>
        /// Resolves the header image for a customer.
        /// </summary>
        /// <param name="clientKey">the customer key</param>
        /// <returns>The header image relative path</returns>
        public static string ResolveHeaderImagePath(string clientKey) {
            //otb image
            var headerImageUrl = "Content/Images/header-email.jpg";

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(baseDirectory + "//Content//Customers//" + clientKey + "//images//header-email.jpg")) {
                headerImageUrl = "Content/Customers/" + clientKey + "/images/header-email.jpg";
            } else if (File.Exists(baseDirectory + "//Content//Images//" + clientKey + "//header-email.jpg")) {
                headerImageUrl = "Content/Images/" + clientKey + "/header-email.jpg";
            }

            return headerImageUrl;
        }
    }
}