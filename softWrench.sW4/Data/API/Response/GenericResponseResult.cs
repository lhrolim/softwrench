using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.API.Response {
    public class GenericResponseResult<T> : IGenericResponseResult {

        /// <summary>
        /// Use this dictionary to append extra properties that could be used by application custom sections
        /// </summary>
        private IDictionary<string, object> _extraParameters = new Dictionary<string, object>();

        public GenericResponseResult() {

        }

        public GenericResponseResult(T resultObject, string successMessage = null) {
            ResultObject = resultObject;
            SuccessMessage = successMessage;
        }
        public string AliasURL { get; set; }
        public T ResultObject { get; set; }
        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public DateTime TimeStamp { get; set; }

        public string Type => GetType().Name;

        public IList<AttributeHolder> ToList() {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> ExtraParameters {
            get { return _extraParameters; }
            set { _extraParameters = value; }
        }


    }
}
