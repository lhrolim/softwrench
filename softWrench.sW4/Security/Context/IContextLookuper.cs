using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Security.Context {
    public interface IContextLookuper :ISingletonComponent {
        ContextHolder LookupContext();

        void FillContext(ApplicationMetadataSchemaKey key);
        void SetInternalQueryExecution();
    }
}