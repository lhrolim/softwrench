using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using log4net;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sw4.Shared2.Metadata.Exception;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Schema {
    public class SchemaFilterBuilder {

        private static readonly ILog Log = LogManager.GetLogger(typeof(SchemaFilterBuilder));



        /// <summary>
        /// For each of the fields, of the overriden schema, if their position is null, add it to the final of the list;
        /// otherwise, either replace the original field with it´s customized version, or append it, before/after the selected element
        /// </summary>
        /// <param name="originalSchemaFilters"></param>
        /// <param name="overridenSchemaFilters"></param>
        public static SchemaFilters ApplyFilterCustomizations(SchemaFilters originalSchemaFilters, SchemaFilters overridenSchemaFilters) {
            var overridenFilters = overridenSchemaFilters;
            if (overridenFilters.IsEmpty()) {
                return originalSchemaFilters; 
            }
            foreach (var overridenFilter in overridenFilters.Filters) {
                var position = overridenFilter.Position;
                var originalFilters = originalSchemaFilters.Filters;
                var attributeOverridingFilter = originalFilters.FirstOrDefault(f => f.Attribute.EqualsIc(overridenFilter.Attribute));

                if (position == null) {
                    if (attributeOverridingFilter == null) {
                        //just adding a brand new filter redeclared on customized schema
                        originalFilters.AddLast(overridenFilter);
                        continue;
                    }
                    position = overridenFilter.Attribute;
                }
                var originalFilter = originalFilters.FirstOrDefault(f => f.Attribute.EqualsIc(position));
                if (originalFilter == null) {
                    continue;
                }
                var originalNode = originalFilters.Find(originalFilter);
                if (originalNode == null) {
                    continue;
                }
                if (position.StartsWith("+")) {
                    originalFilters.AddAfter(originalNode, overridenFilter);
                } else if (position.StartsWith("-")) {
                    originalFilters.AddBefore(originalNode, overridenFilter);
                } else {
                    originalFilters.AddBefore(originalNode, overridenFilter);
                    originalFilters.Remove(originalNode);
                }
            }
            return originalSchemaFilters;
        }


        public static SchemaFilters BuildSchemaFilters(ApplicationSchemaDefinition schema) {
            //linked list since we won´t ever need to locate a filter by it´s index
            var schemaFilters = new LinkedList<BaseMetadataFilter>();

            var entity = MetadataProvider.Entity(schema.EntityName);

            var positionBuffer = AddColumnFilters(schema, schemaFilters, entity);

            AddExtraFilters(schema, schemaFilters, positionBuffer, entity);
            return new SchemaFilters(schemaFilters);
        }


        /// <summary>
        /// N ==> Fields, M = Filters
        ///  O(N x Log(M) ) but M is so small that it´s not a big deal...
        /// </summary>
        /// <param name="schema">schema</param>
        /// <param name="resultSchemaFilters"></param>
        /// <param name="entity"></param>
        private static IDictionary<string, LinkedListNode<BaseMetadataFilter>> AddColumnFilters(ApplicationSchemaDefinition schema, LinkedList<BaseMetadataFilter> resultSchemaFilters, EntityMetadata entity) {

            var declaredFilters = schema.DeclaredFilters;
            var applicationFieldDefinitions = schema.Fields;


            IDictionary<string, LinkedListNode<BaseMetadataFilter>> positionBuffer = new Dictionary<string, LinkedListNode<BaseMetadataFilter>>();
            foreach (var field in applicationFieldDefinitions) {
                
                if (field.IsTransient()) {
                    //transient fields won´t become a filter by default                
                    continue;
                }
                var overridenFilter = declaredFilters.Filters.FirstOrDefault(f => f.Attribute.EqualsIc(field.Attribute));
                if (overridenFilter == null) {
                    if (field.IsHidden) {
                        //first pass, all of non hidden columns become a filter (unless marked to remove)
                        //customized filters can be applied even to hidden fields
                        continue;
                    }

                    //no declared filter let´s just generate a default filter for the column
                    var attr = entity.LocateAttribute(field.Attribute);
                    if (attr == null) {
                        Log.WarnFormat("column {0} not found on entity {1}. Review your metadata", field.Attribute, entity.Name);
                        continue;
                    }

                    var generatedFilter = BaseMetadataFilter.FromField(field.Attribute, field.Label, field.ToolTip, attr.Type);
                    positionBuffer.Add(field.Attribute, resultSchemaFilters.AddLast(generatedFilter));
                } else if (!overridenFilter.Remove) {
                    //merging existing filter
                    overridenFilter.Label = overridenFilter.Label ?? field.Label;
                    overridenFilter.Tooltip = overridenFilter.Tooltip ?? field.ToolTip;
                    if (overridenFilter is MetadataOptionFilter) {
                        ValidateOptionFilter(schema, (MetadataOptionFilter)overridenFilter, entity);
                    }

                    positionBuffer.Add(overridenFilter.Attribute, resultSchemaFilters.AddLast(overridenFilter));
                }
            }
            return positionBuffer;
        }


        ///  <summary>
        /// O(M x Log(N) ) again, M is so small, and usually empty, no need to optimize
        /// second pass all transient filters (i.e extra fields rather than the ones eventually declared on the grid) are added to the list
        ///  </summary>
        /// <param name="schema"></param>
        /// <param name="schemaFilters"></param>
        /// <param name="positionBuffer"></param>
        /// <param name="entity"></param>
        private static void AddExtraFilters(ApplicationSchemaDefinition schema, LinkedList<BaseMetadataFilter> schemaFilters, IDictionary<string, LinkedListNode<BaseMetadataFilter>> positionBuffer, EntityMetadata entity) {

            var declaredFilters = schema.DeclaredFilters;
            var applicationName = schema.ApplicationName;
            var applicationFieldDefinitions = schema.Fields;

            foreach (var filter in declaredFilters.Filters) {
                var field = applicationFieldDefinitions.FirstOrDefault(f => f.Attribute.EqualsIc(filter.Attribute));

                if (field != null) {
                    //this filter has already been added on first step
                    continue;
                }

                if (!filter.IsTransient()) {
                    //non transient filters need to map to an existing attribute
                    Log.WarnFormat("Attribute {0} not found for filter in application {1}. check your metadata".Fmt(filter.Attribute,
                            applicationName));
                    continue;
                }

                if (!filter.IsValid()) {
                    throw new MetadataException(
                        "Filter {0} is not correctly declared for application {1} (missing some fields). check your metadata".Fmt(filter.Attribute,
                            applicationName));
                }

                if (filter is MetadataOptionFilter) {
                    ValidateOptionFilter(schema, (MetadataOptionFilter)filter, entity);
                }

                if (filter.Position == null) {
                    //adding extra field at the end
                    schemaFilters.AddLast(filter);
                    continue;
                }
                var positionalAttribute = filter.Position;
                if (positionalAttribute.StartsWith("-") || positionalAttribute.StartsWith("+")) {
                    positionalAttribute = positionalAttribute.Substring(1);
                }

                var linkedListNode = positionBuffer[positionalAttribute];
                if (linkedListNode == null) {
                    throw new MetadataException("Cannot locate filter {0} to customize on application {1}. Review your metadata".Fmt(positionalAttribute, applicationName));
                }

                if (filter.Position.StartsWith("-")) {
                    schemaFilters.AddBefore(linkedListNode, filter);
                } else if (filter.Position.StartsWith("+")) {
                    schemaFilters.AddAfter(linkedListNode, filter);
                } else {
                    //replacing
                    schemaFilters.AddBefore(linkedListNode, filter);
                    schemaFilters.Remove(linkedListNode);
                }
            }
        }

        /// <summary>
        /// Validates the given provider exists.
        /// 
        /// Note: whereclause cannot be validated since it can be evaluated on mobile time, depending on the nature of the application (ex: @xxx.yyy can be either a simpleinjector or a
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="overridenFilter"></param>
        /// <param name="entity"></param>
        private static void ValidateOptionFilter(ApplicationSchemaDefinition schema, MetadataOptionFilter overridenFilter, EntityMetadata entity) {
            var provider = overridenFilter.Provider;
            // option list provided as static values
            if (string.IsNullOrEmpty(provider)) {
                return;
            }

            if (provider.StartsWith("@")) {

                if (!ClientPlatform.Web.Equals(schema.Platform)) {
                    //nothing to do, because it could be schema shared to the offline world whose implementation will be an angular service
                    return;
                }

                var dataSet = DataSetProvider.GetInstance().LookupDataSet(schema.ApplicationName, schema.SchemaId);
                var mi = ReflectionUtil.GetMethodNamed(dataSet, provider.Substring(1));
                if (mi == null) {
                    throw new FilterMetadataException("missing filter provider method {0} on dataset for application {1}".Fmt(provider.Substring(1), schema.ApplicationName));
                }
                if (mi.GetParameters().Length != 1 || !typeof(IEnumerable<IAssociationOption>).IsAssignableFrom(mi.ReturnType) || !typeof(FilterProviderParameters).IsAssignableFrom(mi.GetParameters()[0].ParameterType)) {
                    //checking signature of the method
                    throw new InvalidOperationException(string.Format("Filter provider {0} at  {1} has wrong signature ", provider.Substring(1), dataSet.GetType().Name));
                }
                return;
            }


            var association = entity.LocateAssociationByLabelField(provider, true).Item1;
            if (association == null) {
                throw new FilterMetadataException("cannot locate a valid filter provider {0} on application {1}. Check the available relationships of the underlying entity {2}".Fmt(provider, schema.ApplicationName, entity.Name));
            }

            if (association.Cacheable) {
                //this means that this filter should render the whole list of options
                overridenFilter.Lazy = false;
            }

        }
    }
}
