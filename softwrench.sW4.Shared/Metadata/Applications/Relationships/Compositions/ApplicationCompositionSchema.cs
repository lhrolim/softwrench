using System;
using softwrench.sW4.Shared.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Compositions {
    public class ApplicationCompositionSchema {

        private readonly String _detailSchema;
        private readonly Boolean _inline;
        private readonly CompositionFieldRenderer _renderer;
        private readonly SchemaMode _renderMode;
        
        public CompositionSchemas Schemas { get; set; }

        public ApplicationCompositionSchema(bool inline, string detailSchema,SchemaMode renderMode, CompositionFieldRenderer renderer) {
            _inline = inline;
            _renderer = renderer;
            _detailSchema = detailSchema;
            _renderMode = renderMode;
        }

        public bool INLINE {
            get { return _inline; }
        }

        public CompositionFieldRenderer Renderer {
            get { return _renderer; }
        }

        public string DetailSchema {
            get { return _detailSchema; }
        }



        public SchemaMode RenderMode {
            get { return _renderMode; }
        }

        public override string ToString() {
            return string.Format("DetailSchema: {0}, INLINE: {1}", _detailSchema, _inline);
        }

        
    }
}
