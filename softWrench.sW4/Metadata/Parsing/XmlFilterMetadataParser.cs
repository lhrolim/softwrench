using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softWrench.sW4.Exceptions;


namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     application metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlFilterMetadataParser {

        [CanBeNull]
        public static SchemaFilters ParseSchemaFilters(XElement schemaElement, SchemaStereotype stereotype) {
            var declaredFilters = schemaElement.Elements().FirstOrDefault(f => f.IsNamed(XmlFilterSchema.FiltersElement));
            if (declaredFilters == null) {
                return null;
            }
            var els = declaredFilters.Elements();
            var xElements = els as XElement[] ?? els.ToArray();
            if (xElements.Any() && stereotype != SchemaStereotype.List) {
                throw new MetadataException("filters can only be declared in list schemas");
            }



            var filters = new LinkedList<BaseMetadataFilter>();
            foreach (var el in xElements) {
                var attribute = el.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableAttributeAttribute, true);
                var label = el.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableLabelAttribute);
                var icon = el.AttributeValue(XmlBaseSchemaConstants.IconAttribute);
                var position = el.AttributeValue(XmlMetadataSchema.CustomizationPositionAttribute);
                var tooltip = el.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute);
                var whereclause = el.AttributeValue(XmlFilterSchema.WhereClauseAttribute);

                if (el.IsNamed(XmlFilterSchema.OptionFilterElement)) {
                    var provider = el.AttributeValue(XmlFilterSchema.ProviderAttribute, true);
                    var allowBlank = el.Attribute(XmlFilterSchema.AllowBlankAttribute).ValueOrDefault(false);
                    var options = ParseDefaultOptions(el);
                    filters.AddLast(new MetadataOptionFilter(attribute, label, icon, position, tooltip, whereclause, provider, allowBlank, options));
                } else if (el.IsNamed(XmlFilterSchema.BooleanFilterElement)) {
                    var defaultValue = el.Attribute(XmlFilterSchema.DefaultSelectionAttribute).ValueOrDefault(true);
                    filters.AddLast(new MetadataBooleanFilter(attribute, label, icon, position, tooltip, whereclause, defaultValue));
                } else if (el.IsNamed(XmlFilterSchema.BaseFilterElement)) {
                    var toRemove = el.Attribute(XmlFilterSchema.RemoveAttribute).ValueOrDefault(true);
                    filters.AddLast(new BaseMetadataFilter(attribute, label, icon, position, tooltip, whereclause, toRemove));
                }
            }
            return new SchemaFilters(filters);

        }

        private static IEnumerable<MetadataFilterOption> ParseDefaultOptions(XElement optionFilterElement) {
            var els = optionFilterElement.Elements().Where(e => e.IsNamed(XmlFilterSchema.OptionElement));
            var xElements = els as XElement[] ?? els.ToArray();
            if (!xElements.Any()) {
                return null;
            }
            ICollection<MetadataFilterOption> options = new LinkedList<MetadataFilterOption>();
            foreach (var el in xElements) {
                var label = el.AttributeValue(XmlBaseSchemaConstants.LabelAttribute, true);
                var value = el.AttributeValue(XmlBaseSchemaConstants.ValueAttribute, true);
                options.Add(new MetadataFilterOption(label, value));
            }
            return options;
        }
    }
}
