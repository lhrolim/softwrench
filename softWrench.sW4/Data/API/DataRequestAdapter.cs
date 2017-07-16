using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API {

    public class DataRequestAdapter : IDataRequest {
        public ApplicationMetadataSchemaKey Key { get; set; }

        public String Title { get; set; }

        public IDictionary<string, string> CustomParameters { get; set; }
        public string CommandId { get; set; }


        public PaginatedSearchRequestDto SearchDTO { get; set; }

        /// <summary>
        /// This parameter is used to open a detail with some initial data, coming from the client side, pre-filled
        /// </summary>
        public JObject InitialData { get; set; }

        public string Id { get; set; }

        public string HmacHash {get; set;}

        //TODO: this is wrong, pick it from custom parameter
        public string Faqid { get; set; }

        //TODO: this is wrong, pick from current user
        public string Lang { get; set; }

        public DataRequestAdapter() {
        }

        public DataRequestAdapter(PaginatedSearchRequestDto searchDTO) {
            SearchDTO = searchDTO;
        }

        public DataRequestAdapter(PaginatedSearchRequestDto searchDTO, ApplicationMetadataSchemaKey key) {
            SearchDTO = searchDTO;
            Key = key;
        }


    }
}
