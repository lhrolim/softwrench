using System;
using softWrench.sW4.Util;

namespace softWrench.sW4.SPF {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SPFRedirectAttribute : Attribute {

        public const string ConventionPattern = "/Content/Controller/{0}.html";

        /// <summary>
        /// Used to specify what´s the TemplateURL that should be load upon the result of this request. Used, only to modify framework´s convention, 
        /// which, by default, would be search for a html with the same name of the controller on the ConventionPattern folder
        /// 
        /// </summary>
        public string URL { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// If this is present, the page will be included inside the crud_body scope instead of a complete different page, making it perform faster
        /// 
        /// //TODO: unify this logic, so that this becomes the only option
        /// </summary>
        public string CrudSubTemplate { get; set; }

        /// <summary>
        /// if present, the redirect url will be marked as null, instead of the default framework convention
        /// </summary>
        public Boolean Avoid { get; set; }

        public SPFRedirectAttribute() {

        }

        public SPFRedirectAttribute(string title, string in18Key, string url = null) {
            Title = new I18NResolver().I18NValue(in18Key, title);
            URL = url;
        }
    }
}