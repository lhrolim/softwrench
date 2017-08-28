using System.Threading.Tasks;
using softWrench.sW4.Configuration.Definitions;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.configuration;

namespace softWrench.sW4.Configuration.Services.Api {

    public interface IConfigurationFacade : IConfigurationFacadeCommons {


        Task RegisterAsync([NotNull]string configKey, [NotNull]PropertyDefinition definition);

        void Register([NotNull]string configKey, [NotNull]PropertyDefinition definition);

        /// <summary>
        /// This method is similar to the register key method but it´s intended to be used only to override an existing definition default value, for instance, for a given customer.
        /// The property definition doesn´t need to be completed declared, and the overriden definition will merge the existing one.
        /// 
        /// Only a single override is allowed for a key.
        /// 
        /// Not intended to be called, except on customer module
        /// 
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="newDefaultValue"></param>
        void Override([NotNull]string configKey, [NotNull]string newDefaultValue);


        Task SetValue([NotNull]string configkey, [NotNull]object value);

        Task<ClientSideConfigurations> GetClientSideConfigurations(long? cacheTimestamp);

        void ConditionAltered(string configKey);
    }
}