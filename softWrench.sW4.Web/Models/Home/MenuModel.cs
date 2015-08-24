using System.Collections.Generic;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;

namespace softWrench.sW4.Web.Models.Home {
    public class MenuModel {

        private readonly MenuDefinition _menu;
        private readonly IDictionary<string, CommandBarDefinition> _commandBars;
        private readonly bool _isSysAdmin;
        private readonly bool _isClientAdmin;

        public MenuModel(MenuDefinition menu, IDictionary<string, CommandBarDefinition> commandBars, bool isSysAdmin, bool isClientAdmin) {
            _menu = menu;
            _isSysAdmin = isSysAdmin;
            _isClientAdmin = isClientAdmin;
            _commandBars = commandBars;
        }

        public MenuDefinition Menu {
            get { return _menu; }
        }

        public bool IsSysAdmin {
            get { return _isSysAdmin; }
        }

        public bool IsClientAdmin {
            get { return _isClientAdmin; }
        }

        public IDictionary<string, CommandBarDefinition> CommandBars {
            get { return _commandBars; }
        }
    }
}
