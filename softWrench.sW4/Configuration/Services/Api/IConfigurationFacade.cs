using System.Threading.Tasks;
using softWrench.sW4.Configuration.Definitions;
using cts.commons.simpleinjector;
using JetBrains.Annotations;

namespace softWrench.sW4.Configuration.Services.Api {

    public interface IConfigurationFacade : ISingletonComponent {

        [CanBeNull]
        T Lookup<T>([NotNull]string configKey);

        Task<T> LookupAsync<T>([NotNull]string configKey);

        Task RegisterAsync([NotNull]string configKey, [NotNull]PropertyDefinition definition);

        void Register([NotNull]string configKey, [NotNull]PropertyDefinition definition);

        Task SetValue([NotNull]string configkey, [NotNull]object value);

        Task<ClientSideConfigurations> GetClientSideConfigurations(long? cacheTimestamp);

        void ConditionAltered(string configKey);
    }
}