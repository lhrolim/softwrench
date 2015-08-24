using cts.commons.simpleinjector.Events;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Menu {
    public class MenuManager : ISWEventListener<ClearMenuEvent> {
        public void HandleEvent(ClearMenuEvent eventToDispatch) {
            SecurityFacade.CurrentUser().ClearMenu();
        }
    }
}
