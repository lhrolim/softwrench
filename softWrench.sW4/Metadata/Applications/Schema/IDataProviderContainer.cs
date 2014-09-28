namespace softWrench.sW4.Metadata.Applications.Schema
{
    public interface IDataProviderContainer :IApplicationDisplayable
    {
        string AssociationKey { get; }
        string Target { get; }
    }
}