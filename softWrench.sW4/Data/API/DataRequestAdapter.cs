using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API {

    public class DataRequestAdapter : IDataRequest {
        public ApplicationMetadataSchemaKey Key { get; set; }

        /// <summary>
        /// If present, this fields will be applied on top of the original schema. Used heavily on dashboards
        /// </summary>
        public string SchemaFieldsToDisplay { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// When passing a dictionary from the front end, the object should be structured like a C# Dictinary.
        /// dictionary[n].key = key;
        /// dictionary[n]value = value
        /// </summary>
        public IDictionary<string, object> CustomParameters { get; set; }
        public string CommandId { get; set; }


        public PaginatedSearchRequestDto SearchDTO { get; set; }




        /// <summary>
        /// This parameter is used to open a detail with some initial data, coming from the client side, pre-filled
        /// </summary>
        public JObject InitialData { get; set; }

        public string Id { get; set; }
        public string UserId { get; set; }

        public string SiteId {get; set;}

        //TODO: this is wrong, pick it from custom parameter
        public string Faqid { get; set; }

        //TODO: this is wrong, pick from current user
        public string Lang { get; set; }

        public string TransactionType { get; set; }

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
