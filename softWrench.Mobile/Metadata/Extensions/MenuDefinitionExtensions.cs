using System.Linq;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;

namespace softWrench.iOS.Mobile.Metadata.Extensions {
    public static class MenuDefinitionExtensions {

        public static bool IsApplicationOnMenu(this MenuDefinition menu, string applicationName) {
            return menu.Leafs.OfType<ApplicationMenuItemDefinition>().Any(appMenu => appMenu.Application == applicationName);
        }
    }
}