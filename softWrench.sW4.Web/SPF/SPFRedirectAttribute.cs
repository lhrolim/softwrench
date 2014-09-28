using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.SPF {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SPFRedirectAttribute : Attribute {

        /// <summary>
        /// Used to specify what´s the TemplateURL that should be load upon the result of this request. Used, only to modify framework´s convention.
        /// </summary>
        public string URL { get; set; }

        public string Title { get; set; }

        public string CrudSubTemplate { get; set; }

        /// <summary>
        /// if present, the redirect url will be marked as null, instead of the default framework convention
        /// </summary>
        public Boolean Avoid { get; set; }

        public string TitleAux { get; set; }

        public SPFRedirectAttribute() {

        }

        public SPFRedirectAttribute(string title, string in18Key, string url = null) {
            Title = new I18NResolver().I18NValue(in18Key, title);
            URL = url;
        }
    }
}