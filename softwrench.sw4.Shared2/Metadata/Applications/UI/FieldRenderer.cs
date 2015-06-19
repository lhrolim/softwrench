﻿using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Util;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.UI {


    public class FieldRenderer {
        private const string WrongRenderer = "renderer {0} not found. Possible options are radio,checkbox,numericinput,datetime,date,time,textarea,screenshot,upload,default,image,treeview,label,richtext,fieldwithbuttons,icon,color,yn,multipleupload,email";

        public FieldRenderer() { }

        private readonly string _parameterString;

        private IDictionary<string, object> _parameters;

        public FieldRenderer(string renderertype, string parameters, string targetName) {
            _parameterString = parameters;
            TargetName = targetName;
            RendererType = renderertype;
            ValidateRendererType(renderertype);
            _parameters = ParametersAsDictionary();
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

        public IDictionary<string, object> Parameters {
            get { return _parameters; }
        }



        public enum BaseRendererType {
            RADIO, NUMERICINPUT, DATETIME, SCREENSHOT, UPLOAD, TEXTAREA, DEFAULT, IMAGE, TREEVIEW, LABEL, CUSTOM, COLOR, RICHTEXT, CHECKBOX, FIELDWITHBUTTONS, ICON, MULTIPLEUPLOAD, EMAIL, DATE, TIME
        }


        public IDictionary<string, object> ParametersAsDictionary() {
            return PropertyUtil.ConvertToDictionary(_parameterString);
        }

        public override string ToString() {
            return string.Format("TargetName: {0}, RendererType: {1}, Parameters: {2}", TargetName, RendererType, Parameters);
        }
    }
}
