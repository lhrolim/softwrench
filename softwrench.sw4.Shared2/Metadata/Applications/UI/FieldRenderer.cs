using softwrench.sW4.Shared2.Util;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.UI {


    public class FieldRenderer {
        private const string WrongRenderer = "renderer {0} not found. Possible options are radio,checkbox,numericinput,datetime,textarea,screenshot,upload,default,image,treeview,label,richtext,fieldwithbuttons,icon,color,yn, multipleupload,email";

        public FieldRenderer() { }

        public FieldRenderer(string renderertype, string parameters, string targetName) {
            Parameters = parameters;
            TargetName = targetName;
            RendererType = renderertype;
            ValidateRendererType(renderertype);
        }

        protected virtual void ValidateRendererType(String rendererType) {
            BaseRendererType result;
            if (!Enum.TryParse(rendererType, true, out result)) {
                throw new InvalidOperationException(String.Format(WrongRenderer, rendererType));
            }
        }


        private string TargetName { get; set; }

        //        private RendererType Renderertype { get; set; }

        public string RendererType { get; set; }

        public string Parameters { get; set; }



        public enum BaseRendererType {
            RADIO,NUMERICINPUT, DATETIME, SCREENSHOT, UPLOAD, TEXTAREA, DEFAULT, IMAGE, TREEVIEW, LABEL, CUSTOM,COLOR, RICHTEXT,CHECKBOX,FIELDWITHBUTTONS, ICON, MULTIPLEUPLOAD,EMAIL
        }


        public IDictionary<string, object> ParametersAsDictionary() {
            return PropertyUtil.ConvertToDictionary(Parameters);
        }

        public override string ToString() {
            return string.Format("TargetName: {0}, RendererType: {1}, Parameters: {2}", TargetName, RendererType, Parameters);
        }
    }
}
