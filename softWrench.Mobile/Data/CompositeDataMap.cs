using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Data {
    public class CompositeDataMap {
        public static CompositeDataMap Wrap(ApplicationSchemaDefinition compositeApplication, DataMap compositeDataMap) {
            return new CompositeDataMap(compositeApplication, compositeDataMap, Enumerable.Empty<Component>());
        }

        public static async Task<CompositeDataMap> Expand(ApplicationSchemaDefinition compositeApplication, DataMap compositeDataMap) {
            var repository = new DataRepository();
            var components = new List<Component>();

            foreach (var composition in compositeApplication.Compositions) {
                var filters = new List<FilterExpression>();

                // Handles the composition criteria, which can
                // be either literals ("hardcoded") values or
                // based on the data map current state.
                foreach (var criterion in composition.EntityAssociation.Attributes) {
                    var value = string.IsNullOrEmpty(criterion.Literal)
                        ? compositeDataMap.Value(criterion.From)
                        : criterion.Literal;

                    filters.Add(new Exactly(criterion.To, value));
                }

                // Fetches all data maps ("components") that
                // satisfies the composition criteria.
                var componentApplication = composition.To();
                var componentDataMaps = (await repository
                    .LoadAsync(componentApplication, filters))
                    .ToList();

                components.Add(new Component(componentApplication, componentDataMaps));
            }

            return new CompositeDataMap(compositeApplication, compositeDataMap, components);
        }

        private readonly ApplicationSchemaDefinition _application;
        private readonly DataMap _composite;
        private readonly IDictionary<ApplicationSchemaDefinition, Component> _components;

        private CompositeDataMap(ApplicationSchemaDefinition application, DataMap composite, IEnumerable<Component> components) {
            if (application == null) throw new ArgumentNullException("application");
            if (composite == null) throw new ArgumentNullException("composite");
            if (components == null) throw new ArgumentNullException("components");

            _application = application;
            _composite = composite;
            _components = components.ToDictionary(k => k.Application);
        }

        public void AddComponent(ApplicationSchemaDefinition componentApplication, DataMap componentDataMap) {
            // Ensures the component's application is
            // indeed defined in the composite metadata.
            if (false == Application
                .Compositions
                .Any(c => componentApplication.Equals(c.To()))) {
                throw new InvalidOperationException();
            }

            Component component;
            if (false == _components.TryGetValue(componentApplication, out component)) {
                component = new Component(componentApplication, new List<DataMap>());
                _components[componentApplication] = component;
            }

            component.DataMaps.Insert(0, componentDataMap);
        }

        public IReadOnlyList<DataMap> Components(ApplicationSchemaDefinition application) {
            return _components[application].DataMaps;
        }

        public IReadOnlyList<DataMap> Components(ApplicationCompositionDefinition composition) {
            return Components(composition.To());
        }

        public IEnumerable<Component> Components() {
            return _components.Values;
        }

        public DataMap Composite {
            get { return _composite; }
        }

        public ApplicationSchemaDefinition Application {
            get { return _application; }
        }

        public sealed class Component {
            private readonly ApplicationSchemaDefinition _application;
            private readonly List<DataMap> _dataMaps;

            public Component(ApplicationSchemaDefinition application, List<DataMap> dataMaps) {
                _application = application;
                _dataMaps = dataMaps;
            }

            public ApplicationSchemaDefinition Application {
                get { return _application; }
            }

            public List<DataMap> DataMaps {
                get { return _dataMaps; }
            }
        }
    }
}
