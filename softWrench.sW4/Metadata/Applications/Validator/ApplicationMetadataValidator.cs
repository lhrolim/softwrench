using System;
using System.Collections;
using System.Collections.Generic;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using log4net;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Validator {
    public class ApplicationMetadataValidator : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationMetadataValidator));
        private const string OptionProviderAlias = "Option Provider";
        private const string CompositionPreFilterAlias = "Composition Pre Filter";
        private const string AssociationPreFilterAlias = "Association Pre Filter";
        private const string AssociationPostFilterAlias = "Association Post Filter";
        private static readonly List<MethodInvocation> OptionProviders = new List<MethodInvocation>();
        private static readonly List<MethodInvocation> CompositionPreFilters = new List<MethodInvocation>();
        private static readonly List<MethodInvocation> AssociationPreFilters = new List<MethodInvocation>();
        private static readonly List<MethodInvocation> AssociationPostFilters = new List<MethodInvocation>();
        private static bool _started;
        private static GenericSwMethodInvoker.ParameterData _optionProviderParameterData;
        private static GenericSwMethodInvoker.ParameterData _compositionPreFilterParameterData;
        private static GenericSwMethodInvoker.ParameterData _associationPreFilterParameterData;
        private static GenericSwMethodInvoker.ParameterData _associationPostFilterParameterData;

        public ApplicationMetadataValidator() {
            if (_started) {
                return;
            }
            _started = true;
            _optionProviderParameterData = new GenericSwMethodInvoker.ParameterData(OptionProviderAlias, typeof(IEnumerable), typeof(OptionFieldProviderParameters));
            _compositionPreFilterParameterData = new GenericSwMethodInvoker.ParameterData(CompositionPreFilterAlias, typeof(SearchRequestDto), typeof(CompositionPreFilterFunctionParameters));
            _associationPreFilterParameterData = new GenericSwMethodInvoker.ParameterData(CompositionPreFilterAlias, typeof(SearchRequestDto), typeof(AssociationPreFilterFunctionParameters));
            _associationPostFilterParameterData = new GenericSwMethodInvoker.ParameterData(CompositionPreFilterAlias, typeof(IEnumerable), typeof(AssociationPostFilterFunctionParameters));
        }

        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName == "pae" || ApplicationConfiguration.ClientName == "hapag") {
                Log.Debug("Skipping Metadata Validation");
                return;
            }
            Log.Debug("Starting Metadata Validation");
            Log.Debug("Validating Option Providers");
            OptionProviders.ForEach(mi => ValidateMethodInvocation(OptionProviderAlias, mi, _optionProviderParameterData));
            Log.Debug("Validating Composition Pre Filters");
            CompositionPreFilters.ForEach(mi => ValidateMethodInvocation(CompositionPreFilterAlias, mi, _compositionPreFilterParameterData));
            Log.Debug("Validating Association Pre Filters");
            AssociationPreFilters.ForEach(mi => ValidateMethodInvocation(AssociationPreFilterAlias, mi, _associationPreFilterParameterData));
            Log.Debug("Validating Association Post Filters");
            AssociationPostFilters.ForEach(mi => ValidateMethodInvocation(AssociationPostFilterAlias, mi, _associationPostFilterParameterData));
        }

        public static void AddOptionProviderToValidate(string applicationName, string schemaId, string methodName) {
            AddMethodInvocationToValidate(OptionProviders, OptionProviderAlias, applicationName, schemaId, methodName);
        }

        private static void RemoveOptionProviderToValidate(string applicationName, string schemaId, string methodName) {
            RemoveMethodInvocationToValidate(OptionProviders, OptionProviderAlias, applicationName, schemaId, methodName);
        }

        public static void AddCompositionPreFilterToValidate(string applicationName, string schemaId, string methodName) {
            AddMethodInvocationToValidate(CompositionPreFilters, CompositionPreFilterAlias, applicationName, schemaId, methodName);
        }

        private static void RemoveCompositionPreFilterToValidate(string applicationName, string schemaId, string methodName) {
            RemoveMethodInvocationToValidate(CompositionPreFilters, CompositionPreFilterAlias, applicationName, schemaId, methodName);
        }

        public static void AddAssociationPreFilterToValidate(string applicationName, string schemaId, string methodName) {
            AddMethodInvocationToValidate(AssociationPreFilters, AssociationPreFilterAlias, applicationName, schemaId, methodName);
        }

        public static void RemoveAssociationPreFilterToValidate(string applicationName, string schemaId, string methodName) {
            RemoveMethodInvocationToValidate(AssociationPreFilters, AssociationPreFilterAlias, applicationName, schemaId, methodName);
        }

        public static void AddAssociationPostFilterToValidate(string applicationName, string schemaId, string methodName) {
            AddMethodInvocationToValidate(AssociationPostFilters, AssociationPostFilterAlias, applicationName, schemaId, methodName);
        }

        public static void RemoveAssociationPostFilterToValidate(string applicationName, string schemaId, string methodName) {
            RemoveMethodInvocationToValidate(AssociationPostFilters, AssociationPostFilterAlias, applicationName, schemaId, methodName);
        }

        public static void AddDisplaybleToValidateIfNeeded(ApplicationSchemaDefinition schema, IApplicationDisplayable displayable) {
            AddRemoveDisplaybleToValidateIfNeeded(schema, displayable, true);
        }

        public static void RemoveDisplaybleToValidateIfNeeded(ApplicationSchemaDefinition schema, IApplicationDisplayable displayable) {
            AddRemoveDisplaybleToValidateIfNeeded(schema, displayable, false);
        }

        private static void AddRemoveDisplaybleToValidateIfNeeded(ApplicationSchemaDefinition schema,
            IApplicationDisplayable displayable, bool add) {

            // if displayable is a container digs for other displayables
            var container = displayable as IApplicationDisplayableContainer;
            if (container != null && container.Displayables != null) {
                container.Displayables.ForEach(d => AddRemoveDisplaybleToValidateIfNeeded(schema, d, add));
                return;
            }

            // option
            var optionField = displayable as OptionField;
            if (optionField != null && optionField.ProviderAttribute != null) {
                if (add) {
                    AddOptionProviderToValidate(schema.ApplicationName, schema.SchemaId, optionField.ProviderAttribute);
                } else {
                    RemoveOptionProviderToValidate(schema.ApplicationName, schema.SchemaId, optionField.ProviderAttribute);
                }
                return;
            }

            // association prefilter and postfilter
            var association = displayable as ApplicationAssociationDefinition;
            if (association != null && association.Schema != null && association.Schema.DataProvider != null) {
                var dataprovider = association.Schema.DataProvider;
                if (dataprovider.PreFilterFunctionName != null) {
                    if (add) {
                        AddAssociationPreFilterToValidate(schema.ApplicationName, schema.SchemaId,
                            dataprovider.PreFilterFunctionName);
                    } else {
                        RemoveAssociationPreFilterToValidate(schema.ApplicationName, schema.SchemaId,
                           dataprovider.PreFilterFunctionName);
                    }
                }
                if (dataprovider.PostFilterFunctionName == null) {
                    return;

                }
                if (add) {
                    AddAssociationPostFilterToValidate(schema.ApplicationName, schema.SchemaId,
                        dataprovider.PostFilterFunctionName);
                } else {
                    RemoveAssociationPostFilterToValidate(schema.ApplicationName, schema.SchemaId,
                       dataprovider.PostFilterFunctionName);
                }
                return;
            }

            // composition prefilter
            var composition = displayable as ApplicationCompositionDefinition;
            if (composition == null || composition.Schema == null) {
                return;
            }

            var collectionSchema = composition.Schema as ApplicationCompositionCollectionSchema;
            if (collectionSchema == null) {
                return;
            }

            if (collectionSchema.PrefilterFunction == null) {
                return;
            }

            if (add) {
                AddCompositionPreFilterToValidate(schema.ApplicationName, schema.SchemaId,
                    collectionSchema.PrefilterFunction);
            } else {
                RemoveCompositionPreFilterToValidate(schema.ApplicationName, schema.SchemaId,
                    collectionSchema.PrefilterFunction);
            }
        }

        private static void ValidateMethodInvocation(string alias, MethodInvocation methodInvocation, GenericSwMethodInvoker.ParameterData parameterData) {
            var appName = methodInvocation.ApplicationName;
            var schemaId = methodInvocation.SchemaId;
            var method = methodInvocation.MethodName;
            GenericSwMethodInvoker.CheckExistenceByString(appName, schemaId, method, parameterData);
            Log.Debug(string.Format("{0}: {1} - validated", alias, methodInvocation));
        }

        private static void AddMethodInvocationToValidate(ICollection<MethodInvocation> container, string alias, string applicationName, string schemaId, string methodName) {
            AddRemoveMethodInvocationToValidate(container, alias, applicationName, schemaId, methodName, true);
        }

        private static void RemoveMethodInvocationToValidate(ICollection<MethodInvocation> container, string alias, string applicationName, string schemaId, string methodName) {
            AddRemoveMethodInvocationToValidate(container, alias, applicationName, schemaId, methodName, false);
        }

        private static void AddRemoveMethodInvocationToValidate(ICollection<MethodInvocation> container, string alias,
            string applicationName, string schemaId, string methodName, bool add) {
            var mi = new MethodInvocation() {
                Alias = alias,
                ApplicationName = applicationName,
                SchemaId = schemaId,
                MethodName = methodName
            };

            if (add && !container.Contains(mi)) {
                container.Add(mi);
            }

            if (!add && container.Contains(mi)) {
                container.Remove(mi);
            }
        }

        private class MethodInvocation {
            public string Alias { private get; set; }
            public string ApplicationName { get; set; }
            public string SchemaId { get; set; }
            public string MethodName { get; set; }
            public override string ToString() {
                return string.Format("{0}: Application ({1}), Schema ({2}), Method ({3})", Alias, ApplicationName, SchemaId, MethodName);
            }

            private bool Equals(MethodInvocation other) {
                return string.Equals(ApplicationName, other.ApplicationName) && string.Equals(SchemaId, other.SchemaId) && string.Equals(MethodName, other.MethodName);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MethodInvocation)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (ApplicationName != null ? ApplicationName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (SchemaId != null ? SchemaId.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public static void Clear() {
            OptionProviders.Clear();
            CompositionPreFilters.Clear();
            AssociationPreFilters.Clear();
            AssociationPostFilters.Clear();
        }

        // after all defaults (int.MaxValue - 100) but with space to others be after
        // needs to be after the dataset providers
        public int Order { get { return int.MaxValue - 50; } }
    }
}
