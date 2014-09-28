using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata {
    public class FieldRendererMetadata {
        
        private string _renderertype;
        private readonly string _dataprovider;
        private readonly string _parameters;
        private string _fieldAttribute;

//        private AssociationMetadata _associationMetadata;

        public FieldRendererMetadata(string renderertype, string dataprovider, string parameters) {
            _renderertype = renderertype;
            _dataprovider = dataprovider;
            _parameters = parameters;
            if (_parameters != null) {
                _parameters = _parameters.Trim();
            }
        }

        public IDictionary<string, string> InitRenderer(String fieldType, String fieldAttribute) {
            this._fieldAttribute = fieldAttribute;
            if (String.Equals(fieldType, "MXDateTimeType", StringComparison.CurrentCultureIgnoreCase)) {
                _renderertype = "datetime";
            }
            return ParametersAsDictionary();
        }

        public string Renderertype {
            get { return _renderertype; }
        }

        public string Dataprovider {
            get { return _dataprovider; }
        }

        public string Parameters {
            get { return _parameters; }
        }

        public class NullFieldRendererMetadata : FieldRendererMetadata {
            public NullFieldRendererMetadata()
                : base(null, null, null) {
            }
        }

        private IDictionary<string, string> ParametersAsDictionary() {
            if (String.IsNullOrEmpty(_parameters)) {
                return new Dictionary<string, string>();
            }

            var result = new Dictionary<string, string>();
            string[] paramSplitArr = _parameters.Split(';');
            foreach (var param in paramSplitArr) {
                if (String.IsNullOrEmpty(param)) {
                    continue;
                }
                if (param.IndexOf("=", System.StringComparison.Ordinal) == -1) {
                    throw new ArgumentException(String.Format("Error in field {0} declaration .Renderer parameter must be of key=value template, but was {1}", _fieldAttribute, _parameters));
                }
                string[] strings = param.Split('=');
                result[strings[0]] = strings[1];
            }
            return result;
        }
    }
}
