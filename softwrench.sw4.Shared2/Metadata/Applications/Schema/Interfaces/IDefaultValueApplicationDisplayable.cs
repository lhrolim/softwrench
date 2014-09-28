namespace softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces
{
    public interface IDefaultValueApplicationDisplayable :IApplicationDisplayable
    {
        string DefaultValue { get; }
        string Attribute { get; }
    }
}