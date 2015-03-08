using System.Security.Principal;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.user;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;

namespace softwrench.sw4.api.classes
{
    public interface IMenuManager :IComponent
    {
        MenuDefinition ModifyMenu(MenuDefinition securedMenu,ISWUser principal); 
    }
}