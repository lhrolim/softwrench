using System;
using System.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System.Collections.Generic;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class ApplicationCompositionSchema {

        private String _detailSchema;
        private Boolean _inline;

        protected HashSet<string> _dependantFields = new HashSet<string>();

        public string PrintSchema { get; set; }

        public ApplicationCompositionSchema() { }

        public CompositionSchemas Schemas { get; set; }

        public IDictionary<string, ApplicationEvent> Events {
            get { return _events; }
            set { _events = value; }
        }
     

        private IDictionary<String, ApplicationEvent> _events = new Dictionary<string, ApplicationEvent>();
        private ISet<ApplicationEvent> _eventsSet;

        public ApplicationCompositionSchema(bool inline, string detailSchema, SchemaMode renderMode, CompositionFieldRenderer renderer, 
            string printSchema, string dependantfields, ISet<ApplicationEvent> events = null ) {
            _inline = inline;
            Renderer = renderer;
            _detailSchema = detailSchema;
            PrintSchema = printSchema;
            RenderMode = renderMode;
            _eventsSet = events;
            if (events != null) {
                _events = events.ToDictionary(f => f.Type, f => f);
            }
            if (dependantfields != null) {
                var fields = dependantfields.Split(',');
                foreach (var field in fields) {
                    _dependantFields.Add(field);
                }
            }
        }

        public string DetailSchema {
            get { return _detailSchema; }
            set { _detailSchema = value; }
        }        


        public bool INLINE {
            get { return _inline; }
            set { _inline = value; }
        }

        public CompositionFieldRenderer Renderer { get; set; }

        public IDictionary<string, string> RendererParameters {
            get { return Renderer == null ? new Dictionary<string, string>() : Renderer.ParametersAsDictionary(); }
        }

        public SchemaMode RenderMode { get; set; }


        public HashSet<string> DependantFields {
            get { return _dependantFields; }
        }

        public override string ToString() {
            return string.Format("DetailSchema: {0}, INLINE: {1}", _detailSchema, _inline);
        }


    }
}
