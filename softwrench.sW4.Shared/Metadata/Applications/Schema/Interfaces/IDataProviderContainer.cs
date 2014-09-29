using softwrench.sW4.Shared.Metadata.Applications.Schema;
using softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces;

namespace softWrench.sW4.Metadata.Applications.Schema
{
    public interface IDataProviderContainer :IApplicationDisplayable
    {
        string AssociationKey { get; }
        string Target { get; }
    }
}