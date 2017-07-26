using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System.Collections.Generic;
using System.ComponentModel;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class ApplicationCompositionSchema : IPCLCloneable {
        private bool _inline;

        protected HashSet<string> _dependantFields = new HashSet<string>();

        [DefaultValue("detail")]
        public string PrintSchema {
            get; set;
        }

        public ApplicationCompositionSchema() {
            Events = new Dictionary<string, ApplicationEvent>();
        }

        public CompositionSchemas Schemas {
            get; set;
        }

        public IDictionary<string, ApplicationEvent> Events {
            get; set;
        }


        private ISet<ApplicationEvent> _eventsSet;
        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(FetchType.Lazy)]
        public FetchType FetchType {
            get; set;
        }

        public ApplicationCompositionSchema(bool inline, string detailSchema,string detailOutputSchema, SchemaMode renderMode, CompositionFieldRenderer renderer,
            string printSchema, string dependantfields, FetchType fetchType, ISet<ApplicationEvent> events = null) {
            Events = new Dictionary<string, ApplicationEvent>();
            _inline = inline;
            Renderer = renderer;
            DetailSchema = detailSchema;
            DetailOutputSchema = detailOutputSchema;
            PrintSchema = printSchema;
            RenderMode = renderMode;
            _eventsSet = events;
            FetchType = fetchType;
            if (events != null) {
                Events = events.ToDictionary(f => f.Type, f => f);
            }
            if (dependantfields != null) {
                var fields = dependantfields.Split(',');
                foreach (var field in fields) {
                    _dependantFields.Add(field);
                }
            }

            OriginalDependantfields = dependantfields;
            OriginalEvents = events;
        }

        protected ISet<ApplicationEvent> OriginalEvents {
            get; set;
        }

        public string OriginalDependantfields {
            get; set;
        }

        [DefaultValue("detail")]
        public string DetailSchema { get; set; }

        public string DetailOutputSchema { get; set; }



        public bool INLINE {
            get {
                return _inline;
            }
            set {
                _inline = value;
            }
        }

        public CompositionFieldRenderer Renderer {
            get; set;
        }

        public IDictionary<string, object> RendererParameters {
            get {
                return Renderer == null ? new Dictionary<string, object>() : Renderer.ParametersAsDictionary();
            }
        }
        [DefaultValue(SchemaMode.None)]
        public SchemaMode RenderMode {
            get; set;
        }


        public HashSet<string> DependantFields {
            get {
                return _dependantFields;
            }
        }

        public string RequiredRelationshipExpression {
            get; set;
        }

        public override string ToString() {
            return string.Format("DetailSchema: {0}, INLINE: {1}", DetailSchema, _inline);
        }

        public virtual object Clone() {
            return new ApplicationCompositionSchema(INLINE, DetailSchema,DetailOutputSchema, RenderMode, Renderer, PrintSchema, OriginalDependantfields, FetchType, OriginalEvents);
        }
    }
}
