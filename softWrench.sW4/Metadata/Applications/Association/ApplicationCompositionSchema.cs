using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Applications.Relationship.Composition;

namespace softWrench.sW4.Metadata.Applications.Association {

    public class ApplicationCompositionSchema {
        
        /// <summary>
        /// If true, puts the composition field on the same screen as the original application. 
        /// If false, the composition fields will be encapsulated on an isolated tab.
        /// Defaults false
        /// </summary>
        /// 
        private readonly bool _inline;
        private readonly CompositionFieldRenderer _renderer;

        public ApplicationCompositionSchema(bool inline, CompositionFieldRenderer renderer) {
            _inline = inline;
            _renderer = renderer;
        }

        public bool Inline {
            get { return _inline; }
        }

        public CompositionFieldRenderer Renderer {
            get { return _renderer; }
        }
    }
}
