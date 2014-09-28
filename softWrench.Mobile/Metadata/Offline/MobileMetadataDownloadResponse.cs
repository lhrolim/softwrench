using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.Mobile.Metadata.Offline {
    class MobileMetadataDownloadResponse {
        private readonly MenuDefinition _menu;
        private readonly IEnumerable<CompleteApplicationMetadataDefinition> _metadatas;

        public MobileMetadataDownloadResponse(MenuDefinition menu, IEnumerable<CompleteApplicationMetadataDefinition> metadatas) {
            _menu = menu;
            _metadatas = metadatas;
        }

        public MenuDefinition Menu {
            get { return _menu; }
        }

        public IEnumerable<CompleteApplicationMetadataDefinition> Metadatas {
            get { return _metadatas; }
        }
    }
}
