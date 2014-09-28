using System;
using System.Collections.Specialized;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Web.Http.Controllers;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using IValueProvider = System.Web.Http.ValueProviders.IValueProvider;
using ValueProviderFactory = System.Web.Http.ValueProviders.ValueProviderFactory;
using ValueProviderResult = System.Web.Http.ValueProviders.ValueProviderResult;

namespace softWrench.sW4.Web.Controllers {
    public class DataRequestValueProviderFactory : ValueProviderFactory {




        public override IValueProvider GetValueProvider(HttpActionContext actionContext) {

            
            return new DataControllerValueProvider(actionContext.Request.RequestUri.Query);
        }


       

        class DataControllerValueProvider : IValueProvider
        {

            private readonly string _query;
            private NameValueCollection _nvc;

            public DataControllerValueProvider(string query) {
                _nvc = HttpUtility.ParseQueryString(query);
                _query = query;
//                _json= new JavaScriptSerializer().Serialize(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
            }

//            static Dictionary<string, object> NvcToDictionary(NameValueCollection nvc, bool handleMultipleValuesPerKey) {
//                var result = new Dictionary<string, object>();
//                foreach (string key in nvc.Keys) {
//                    if (handleMultipleValuesPerKey) {
//                        string[] values = nvc.GetValues(key);
//                        if (values.Length == 1) {
//                            result.Add(key, values[0]);
//                        }
//                        else {
//                            result.Add(key, values);
//                        }
//                    }
//                    else {
//                        result.Add(key, nvc[key]);
//                    }
//                }
//
//                return result;
//            }


            public bool ContainsPrefix(string prefix) {
                return true;
            }

            public ValueProviderResult GetValue(string key)
            {
                Type toUse = _query.Contains("Id=") ? typeof (DetailRequest) : typeof (PaginatedSearchRequestDto);
                object value = new QueryStringConverter().ConvertStringToValue(_query, toUse);
                return new ValueProviderResult(value,null,null);
            }
        }


    }
}