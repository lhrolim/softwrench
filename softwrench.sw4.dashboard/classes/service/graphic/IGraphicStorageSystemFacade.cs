using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.user;

namespace softwrench.sw4.dashboard.classes.service.graphic {
    /// <summary>
    /// Facade for communicating with an external Graphic Storage system.
    /// </summary>
    public interface IGraphicStorageSystemFacade : IComponent {
        /// <summary>
        /// Name of the Graphic Storage System the instance connects to.
        /// </summary>
        /// <returns></returns>
        string SystemName();
        /// <summary>
        /// Authenticates the user to the Graphic Strage System.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        IGraphicStorageSystemAuthDto Authenticate(ISWUser user);
        /// <summary>
        /// Loads some resource from the graphic storage system (maybe through an api).
        /// Returns the resource as a string so it can be serialization/format agnostic, 
        /// it's up to the client to handle the interpretation of the response.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        string LoadExternalResource(string resource, JObject request);
    }
}
