using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API {
    public class DetailRequest : IDataRequest, IAssociationPrefetcherRequest {

        private string _id;
        private string _faqid;
        private string _lang;
        public ApplicationMetadataSchemaKey Key {
            get; set;
        }

        public String Title {
            get; set;
        }

        public IDictionary<string, string> CustomParameters {
            get; set;
        }

        public Entity InitialValues {
            get; set;
        }


        public string CommandId {
            get; set;
        }

        public string AssociationsToFetch {
            get; set;
        }


        public DetailRequest() {
        }

        public DetailRequest(ApplicationMetadataSchemaKey key, IDictionary<string, string> customParameters) {
            Key = key;
            CustomParameters = customParameters;
        }

        public DetailRequest(string id, ApplicationMetadataSchemaKey key) {
            _id = id;
            Key = key;
        }

        public DetailRequest(string id, string faqid, string lang, ApplicationMetadataSchemaKey key) {
            _id = id;
            _lang = lang;
            _faqid = faqid;
            Key = key;
        }

        public string Lang {
            get {
                return _lang == "undefined" ? null : _lang;
            }
            set {
                _lang = value;
            }
        }


        public string Faqid {
            get {
                return _faqid == "undefined" ? null : _faqid;
            }
            set {
                _faqid = value;
            }
        }

        public string Id {
            get {
                return _id == "undefined" ? null : _id;
            }
            set {
                _id = value;
            }
        }

        public Tuple<string, string> UserIdSitetuple {
            get; set;
        }

        /// <summary>
        /// accepts #all, or a comma separated composition name list
        /// </summary>
        public string CompositionsToFetch {
            get; set;
        }

        public static DetailRequest GetInstance(ApplicationMetadata applicationMetadata, DataRequestAdapter adapter) {
            var request = new DetailRequest(adapter.Id, adapter.Faqid, adapter.Lang, adapter.Key) {
                CustomParameters = adapter.CustomParameters
            };

            var entityMetadata = MetadataProvider.Entity(applicationMetadata.Entity);
            if (adapter.InitialData != null) {
                var crudData = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), entityMetadata,
                    applicationMetadata, adapter.InitialData, adapter.Id);
                request.InitialValues = crudData;
            }
            return request;
        }

        public bool IsEditionRequest {

            get {
                return Id != null || UserIdSitetuple != null;
            }
        }


    }
}
