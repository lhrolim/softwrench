namespace softWrench.sW4.Data.Search {
    public class SearchParameterUtils {
        private readonly string _param;
        private readonly object _paramValue;
        private readonly string _fieldType;

        public SearchParameterUtils(string param, object paramValue, string fieldType) {
            _param = param;
            _paramValue = paramValue;
            _fieldType = fieldType;
        }

        public string Param {
            get { return _param; }
        }

        public object ParamValue {
            get { return _paramValue; }
        }

        public string FieldType {
            get { return _fieldType; }
        }
    }
}
