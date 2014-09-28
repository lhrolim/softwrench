using System;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using SQLite;

namespace softWrench.Mobile.Persistence {
    [Table("Menu")]
    public class PersistableMenu {
        public PersistableMenu(MenuDefinition menu) {
            Id = 1;
            Data = menu.ToJson();
        }

        public PersistableMenu() {
        }

        [PrimaryKey]
        public Int32 Id { get; set; }

        public string Data { get; set; }
    }
}

