using System.ComponentModel;
using IComponent = softWrench.sW4.SimpleInjector.IComponent;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    /// <summary>
    /// marker interface for classes that wish to provide methods for the applications
    /// </summary>
    public interface IDataSet : IComponent {
        string ApplicationName();
        string ClientFilter();
    }
}