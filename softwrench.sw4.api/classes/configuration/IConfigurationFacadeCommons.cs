using System.Threading.Tasks;
using cts.commons.simpleinjector;
using JetBrains.Annotations;

namespace softwrench.sw4.api.classes.configuration
{
    public interface IConfigurationFacadeCommons : ISingletonComponent
    {

        Task<T> LookupAsync<T>([NotNull]string configKey);

        Task RegisterAsync([NotNull]string configKey, [NotNull]PropertyDefinitionRegistry definition);

    }
}