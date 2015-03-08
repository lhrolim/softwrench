using softWrench.sW4.Configuration.Definitions;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Configuration.Services.Api {

    public interface IConfigurationFacade : ISingletonComponent {

        T Lookup<T>(string configKey);

        void Register(string configKey, PropertyDefinition definition);

        void SetValue(string configkey, object value);


    }
}