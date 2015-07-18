using System.ComponentModel;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Metadata.Applications.DataSet.baseclasses;
using softWrench.sW4.SPF;
using IComponent = softWrench.sW4.SimpleInjector.IComponent;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    /// <summary>
    /// marker interface for classes that wish to provide methods for the applications
    /// </summary>
    public interface IDataSet : IApplicationFiltereable, IComponent {

        CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData);
    }
}