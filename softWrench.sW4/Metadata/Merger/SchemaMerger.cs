﻿using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using log4net;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Applications.Validator;
using softWrench.sW4.Metadata.Parsing;

namespace softWrench.sW4.Metadata.Merger {
    class SchemaMerger {
        private const string NonCustomizableFound = "overriden schemas can only contain customizations, however found {0} wrong displayables ( {1}) for schema {2}";

        private static readonly ILog Log = LogManager.GetLogger(typeof(SchemaMerger));

        public static void MergeSchemas(ApplicationSchemaDefinition original, ApplicationSchemaDefinition overridenSchema, IEnumerable<DisplayableComponent> components) {

            var nonCustomizableDisplayables = GetNonCustomizableFields(overridenSchema);
            if (nonCustomizableDisplayables.Any()) {
                var names = new List<string>();
                foreach (var field in nonCustomizableDisplayables) {
                    var attrfield = field as IApplicationAttributeDisplayable;
                    if (attrfield != null) {
                        names.Add(attrfield.Attribute);
                    }
                }

                throw new MetadataException(NonCustomizableFound.Fmt(nonCustomizableDisplayables.Count(),
                    string.Join(",", names), overridenSchema));
            }
            var customizations = GetCustomizations(overridenSchema);
            var fieldsThatShouldBeCustomized = customizations.Count();
            var customizationsActuallyApplied = new HashSet<int>();
            original.Stereotype = overridenSchema.Stereotype == SchemaStereotype.None
                ? original.Stereotype
                : overridenSchema.Stereotype;

            original.CommandSchema.Merge(overridenSchema.CommandSchema);

            XmlApplicationMetadataParser.AddNoResultsNewSchema(overridenSchema);
            if (overridenSchema.DeclaredNoResultsNewSchema) {
                original.NoResultsNewSchema = overridenSchema.NoResultsNewSchema;
            }
            if (overridenSchema.PreventResultsNewSchema) {
                original.NoResultsNewSchema = null;
            }

            DoApplyCustomizations(original, overridenSchema, components, customizations, customizationsActuallyApplied, fieldsThatShouldBeCustomized);
            SchemaFilterBuilder.ApplyFilterCustomizations(original.SchemaFilters, overridenSchema.DeclaredFilters);
            original.MergeProperties(overridenSchema);

            ApplicationSchemaFactory.AddHiddenRequiredFields(original);
            ApplicationSchemaFactory.AddHiddenSectionControlFields(original);
        }


        private static void DoApplyCustomizations(IApplicationDisplayableContainer original,
            ApplicationSchemaDefinition overridenSchema, IEnumerable<DisplayableComponent> components,
            IList<ApplicationSchemaCustomization> customizations, HashSet<int> customizationsActuallyApplied,
            int fieldsThatShouldBeCustomized) {
            var resultDisplayables = new List<IApplicationDisplayable>();
            foreach (var displayable in original.Displayables) {
                var attrDisplayablee = displayable as IApplicationIndentifiedDisplayable;
                if (attrDisplayablee == null || IsAutoGenerated(attrDisplayablee)) {
                    //can only replace fields that have attributes
                    //TODO: think of what to do with the hidden fields that were generated automatically on associations, and how to handle customizations on these.
                    resultDisplayables.Add(displayable);
                    continue;
                }
                if (displayable is IApplicationDisplayableContainer) {

                    var container = (IApplicationDisplayableContainer)displayable;
                    bool? containerReplaced = null;

                    if (container.Id != null && container is IApplicationIndentifiedDisplayable) {
                        var attrDisplayable = (IApplicationIndentifiedDisplayable)container;
                        //sections, tabs
                        // first we'll try to customize the containers themselves, not their inner fields
                        containerReplaced = DoApplySingleCustomization(overridenSchema, components, customizations, customizationsActuallyApplied, attrDisplayable, resultDisplayables);
                    }

                    if (containerReplaced == null || containerReplaced.Value == false) {
                        //applying customizations on inner fields of the container, since no matching customization was found
                        //now it's time to try to apply customizations to the container inner fields instead
                        DoApplyCustomizations((IApplicationDisplayableContainer)displayable,
                                overridenSchema, components,
                                customizations, customizationsActuallyApplied,
                                fieldsThatShouldBeCustomized);

                        if (containerReplaced == null) {
                            //if the container has an id, then the DoApplySingleCustomization workflow would have already added itself on the resultDisplayables list. We need to avoid such situation 
                            resultDisplayables.Add(displayable);
                        }
                    }
                    continue;
                }

                DoApplySingleCustomization(overridenSchema, components, customizations, customizationsActuallyApplied, attrDisplayablee, resultDisplayables);
            }
            if (customizationsActuallyApplied.Count != fieldsThatShouldBeCustomized && (original is ApplicationSchemaDefinition)) {
                //second condition means that this is not a section iteration, i.e --> check number just at the end
                var names = new List<string>();
                for (var i = 0; i < customizations.Count; i++) {
                    if (!customizationsActuallyApplied.Contains(i)) {
                        names.Add(customizations[i].Position);
                    }
                }
                throw new MetadataException(
                    "customizations {0} could not be applied cause the corresponding fields were not found in schema {1}".Fmt(
                        String.Join(",", names), overridenSchema));
            }
            original.Displayables = resultDisplayables;
        }

        private static bool DoApplySingleCustomization(ApplicationSchemaDefinition overridenSchema, IEnumerable<DisplayableComponent> components,
            IList<ApplicationSchemaCustomization> customizations, HashSet<int> customizationsActuallyApplied, IApplicationIndentifiedDisplayable attrDisplayablee,
            List<IApplicationDisplayable> resultDisplayables) {
            var attribute = attrDisplayablee.Attribute;
            if (attrDisplayablee is IApplicationDisplayableContainer) {
                attribute = ((IApplicationDisplayableContainer)attrDisplayablee).Id;
            }

            var customization = customizations.FirstOrDefault(f => AttributeComparison(f.Position, attribute, attrDisplayablee is ApplicationRelationshipDefinition));
            if (customization == null) {
                if (attrDisplayablee is ApplicationAssociationDefinition) {
                    //if the field is an association let´s give it a change to search for the label field instead before assuming there´s no customization present
                    //this is needed because sometimes we might have multiple fields pointing to a same target and would be preferable to use that strategy, otherwise
                    // both fields would be customized. See materials.xml
                    var association = attrDisplayablee as ApplicationAssociationDefinition;
                    var labelField = association.OriginalLabelField;
                    customization =
                        customizations.FirstOrDefault(f => AttributeComparison(f.Position, labelField, true));
                }
            }

            if (customization == null) {
                //no customization found, add the original field normally
                resultDisplayables.Add(attrDisplayablee);
                return false;
            }

            Log.DebugFormat("applying customization {0} on schema {1}", customization.Position, overridenSchema);
            customizationsActuallyApplied.Add(customizations.IndexOf(customization));

            if (customization.Position.StartsWith("-")) {
                if (!customization.Displayables.Any()) {
                    throw new MetadataException(
                        "left customizations must have a body, check your metadata at {0}  | customization: {1}".Fmt(
                            overridenSchema, customization.Position));
                }
                var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables,
                    overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                resultDisplayables.AddRange(resolvedDisplayables);
                resultDisplayables.Add(attrDisplayablee);
            } else if (customization.Position.StartsWith("+")) {
                if (!customization.Displayables.Any()) {
                    throw new MetadataException(
                        "right customizations must have a body, check your metadata at {0} | customization: {1}".Fmt(
                            overridenSchema, customization.Position));
                }
                resultDisplayables.Add(attrDisplayablee);
                var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables,
                    overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                resultDisplayables.AddRange(resolvedDisplayables);
            } else {
                //exact match
                //if empty this would replace the existing displayable
                var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables,
                    overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                resultDisplayables.AddRange(resolvedDisplayables);

                // removes the replaced displayable from validation
                ApplicationMetadataValidator.RemoveDisplaybleToValidateIfNeeded(overridenSchema, attrDisplayablee);
            }
            return true;
        }

        private static bool AttributeComparison(string position, string attribute, bool isRelationship) {

            var baseComparison = (position.Equals(attribute) || position.Equals("+" + attribute) || position.Equals("-" + attribute));
            if (!baseComparison && isRelationship) {
                //second chance to relationships
                return AttributeComparison(position + "_", attribute, false);
            }
            return baseComparison;
        }


        private static bool IsAutoGenerated(IApplicationIndentifiedDisplayable attrDisplayablee) {
            var fieldDef = attrDisplayablee as ApplicationFieldDefinition;
            return fieldDef != null && fieldDef.AutoGenerated;
        }

        private static IApplicationDisplayable[] GetNonCustomizableFields(ApplicationSchemaDefinition overridenSchema) {
            var result = overridenSchema.Displayables.Where(disp => !(disp is ApplicationSchemaCustomization)).Where(disp => !(disp is ApplicationFieldDefinition) || !((ApplicationFieldDefinition)disp).AutoGenerated).ToList();
            return result.ToArray();
        }

        private static IList<ApplicationSchemaCustomization> GetCustomizations(ApplicationSchemaDefinition overridenSchema) {
            return DisplayableUtil.GetDisplayable<ApplicationSchemaCustomization>(typeof(ApplicationSchemaCustomization), overridenSchema.Displayables);
        }

        public static bool IsCustomized(ApplicationSchemaDefinition overridenSchema) {
            return GetCustomizations(overridenSchema).Count > 0;
        }
    }
}
