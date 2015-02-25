using softwrench.sW4.Shared2.Data;
using softWrench.sW4.SPF;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API {
    public class GenericResponseResult<T> : IGenericResponseResult {

        public GenericResponseResult() {

        }

        public GenericResponseResult(T resultObject, string successMessage = null) {
            ResultObject = resultObject;
            SuccessMessage = successMessage;
        }

        public T ResultObject { get; set; }
        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public long RequestTimeStamp { get; set; }
        public DateTime TimeStamp { get; set; }

        public IList<AttributeHolder> ToList() {
            throw new NotImplementedException();
        }


    }
}
