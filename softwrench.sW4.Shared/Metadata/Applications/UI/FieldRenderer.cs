using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace softwrench.sW4.Shared.Metadata.Applications.UI {


    public class FieldRenderer {
        private const string WrongRenderer = "renderer {0} not found. Possible options are radio,numericinput,datetime,textarea,screenshot,upload,default";

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
                throw new InvalidEnumArgumentException(String.Format(WrongRenderer, rendererType));
            }
        }


        private string TargetName { get; set; }

        //        private RendererType Renderertype { get; set; }

        public string RendererType { get; set; }

        public string Parameters { get; set; }



        public enum BaseRendererType {
            RADIO, NUMERICINPUT, DATETIME,SCREENSHOT,UPLOAD,TEXTAREA,DEFAULT
        }


        public IDictionary<string, string> ParametersAsDictionary() {
            if (String.IsNullOrEmpty(Parameters)) {
                return new Dictionary<string, string>();
            }

            var result = new Dictionary<string, string>();
            string[] paramSplitArr = Parameters.Split(';');
            foreach (var param in paramSplitArr) {
                if (String.IsNullOrEmpty(param)) {
                    continue;
                }
                if (param.IndexOf("=", System.StringComparison.Ordinal) == -1) {
                    throw new ArgumentException(String.Format("Error in field {0} declaration .Renderer parameter must be of key=value template, but was {1}", TargetName, Parameters));
                }
                string[] strings = param.Split('=');
                result[strings[0]] = strings[1];
            }
            return result;
        }

        public override string ToString() {
            return string.Format("TargetName: {0}, RendererType: {1}, Parameters: {2}", TargetName, RendererType, Parameters);
        }
    }
}
