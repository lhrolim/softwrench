using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces
{
    public interface IDataProviderContainer :IApplicationDisplayable
    {
        string AssociationKey { get; }
        string Target { get; }
    }
}