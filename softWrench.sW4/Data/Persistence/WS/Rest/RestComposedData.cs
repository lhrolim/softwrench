using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Operation;

namespace softWrench.sW4.Data.Persistence.WS.Rest {
    public class RestComposedData : IRestObjectWrapper {

        private readonly ICollection<InternalRestComposedData> _composedDataCollection = new List<InternalRestComposedData>();

        private readonly bool _inline;

        public RestComposedData(bool inline = false) {
            _inline = inline;
        }


        public bool Inline {
            get {
                return _inline;
            }
        }


        public InternalRestComposedData AddComposedData(CrudOperationData relatedData, int idx, string relationship) {
            var item = new InternalRestComposedData(relatedData, idx, relationship);
            _composedDataCollection.Add(item);
            return item;
        }

        public bool IsCompositionCreation {
            get {
                return _composedDataCollection.Any(a => a.IsCompositionCreation);
            }

        }

        public string SerializeParameters(string context = null) {
            var sb = new StringBuilder();
            foreach (var composedData in _composedDataCollection) {
                sb.Append(composedData.SerializeParameters(context)).Append("&");
            }

            return sb.ToString(0, sb.Length - 1);
        }


        public void AddEntry(string key, object value) {
            var item = _composedDataCollection.FirstOrDefault();
            if (item != null) {
                item.AddEntry(key, value);
            }
        }

        public IDictionary<string, object> Entries {
            get {
                var item = _composedDataCollection.FirstOrDefault();
                if (item != null) {
                    return item.Entries;
                }
                return null;
            }
        }
    }

    public class InternalRestComposedData : IRestObjectWrapper {

        public InternalRestComposedData(CrudOperationData relatedData, int idx, string relationship) {
            Idx = idx;
            RelatedData = relatedData;
            Relationship = relationship;

            Entries = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (relatedData != null) {
                foreach (var attr in relatedData.Attributes) {
                    Entries[attr.Key] = attr.Value;
                }
                IsCompositionCreation = relatedData.Id == null;
            }


        }

        public int Idx {
            get; set;
        }

        public IDictionary<string, object> Entries {
            get; set;
        }

        public CrudOperationData RelatedData {
            get; set;
        }

        public string Relationship {
            get; set;
        }

        public bool IsCompositionCreation {
            get; set;
        }


        public string SerializeParameters(string context = null) {
            var sb = new StringBuilder();
            var innerContext = GetRestContext();
            var finalContext = innerContext;
            if (context != null) {
                finalContext = context + innerContext;
            }
            foreach (var entry in Entries) {
                if (entry.Value != null) {
                    if (entry.Value is RestComposedData) {
                        var data = (RestComposedData)entry.Value;
                        sb.Append(data.SerializeParameters(finalContext));
                    } else {
                        sb.Append(finalContext)
                            .Append(entry.Key)
                            .Append("=");
                        if (entry.Key == "DOCUMENTDATA") {
                            var base64String = (string)entry.Value;
                            sb.Append(base64String.Replace("+", "%2B"));
                        } else {
                            sb.Append(WebUtility.UrlEncode(entry.Value.ToString()));
                        }
                    }
                }
                sb.Append("&");
            }

            return sb.ToString(0, sb.Length - 1);
        }

        private string GetRestContext() {
            return Relationship + ".id" + Idx + ".";
        }

        public void AddEntry(string key, object value) {
            Entries.Add(key, value);
        }
    }

}
