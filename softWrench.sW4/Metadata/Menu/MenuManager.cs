using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Menu {
    public class MenuManager : ISWEventListener<ClearMenuEvent> {
        public void HandleEvent(ClearMenuEvent eventToDispatch) {
            SecurityFacade.CurrentUser().ClearMenu();
        }
    }
}
