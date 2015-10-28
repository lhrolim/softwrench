using cts.commons.simpleinjector;
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
    }
}
