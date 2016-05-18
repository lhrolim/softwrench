using System;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api.services;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Batches {
    public abstract class ABatchSubmissionConverter : IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper> {
        private readonly EntityMetadata _entityMetadata;
        public abstract string ApplicationName();
        public abstract string ClientFilter();
        public abstract string SchemaId();

        public abstract String BatchProperty { get; }

        protected ABatchSubmissionConverter() {
            if (!ApplicationConfiguration.IsUnitTest) {
                //TODO: rethink
                var appName = ApplicationName();
                var application = MetadataProvider.Application(appName, false);
                if (application == null) {
                    return;
                }

                var entityName = application.Entity;
                _entityMetadata = MetadataProvider.Entity(entityName);
            }
        }


        public virtual JArray BreakIntoRows(JObject mainDatamap) {
            dynamic obj = mainDatamap;
            var rows = obj[BatchProperty];
            mainDatamap.Remove(BatchProperty);

            var result = new JArray();
            foreach (dynamic row in rows) {
                var batchItem = new JObject(mainDatamap);
                foreach (dynamic field in row) {
                    JSonUtil.ReplaceValue(batchItem, field.Name, field.Value);
                }
                result.Add(batchItem);
            }
            return result;
        }

        public virtual bool ShouldSubmit(JObject row) {
            return true;
        }

        public virtual OperationWrapper Convert(JObject row, ApplicationMetadata applicationMetadata) {
            var crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, applicationMetadata, row, null);
            return new OperationWrapper(crudOperationData, OperationConstants.CRUD_CREATE);
        }
    }
}
