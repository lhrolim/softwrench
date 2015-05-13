using System.Text;
using NHibernate.Dialect.Function;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class HapagOfferingLongDescriptionHandler {

        private const string Separator = "=====================================================================";


        public static string ParseSchemaBasedLongDescription(CrudOperationData entity) {
            return ImacDescriptionHandler.BuildDescription(entity, entity.ApplicationMetadata);
        }
    }
}
