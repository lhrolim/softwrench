﻿using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using DocumentFormat.OpenXml.Drawing;
using log4net;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softWrench.sW4.Exceptions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata.Validator {
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


            DoApplyCustomizations(original, overridenSchema, components, customizations, customizationsActuallyApplied, fieldsThatShouldBeCustomized);
            ApplyFilterCustomizations(original, overridenSchema);
        }

        /// <summary>
        /// For each of the fields, of the overriden schema, if their position is null, add it to the final of the list;
        /// otherwise, either replace the original field with it´s customized version, or append it, before/after the selected element
        /// </summary>
        /// <param name="original"></param>
        /// <param name="overridenSchema"></param>
        private static void ApplyFilterCustomizations(ApplicationSchemaDefinition original, ApplicationSchemaDefinition overridenSchema) {
            var overridenFilters = overridenSchema.DeclaredFilters;
            if (overridenFilters.IsEmpty()) {
                return;
            }
            foreach (var overridenFilter in overridenFilters.Filters) {
                var position = overridenFilter.Position;
                var originalFilters = original.SchemaFilters.Filters;
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
                    //sections, tabs
                    DoApplyCustomizations((IApplicationDisplayableContainer)displayable,
                        overridenSchema, components,
                         customizations, customizationsActuallyApplied,
                        fieldsThatShouldBeCustomized);
                    resultDisplayables.Add(displayable);
                    continue;
                }

                var attribute = attrDisplayablee.Attribute;

                var customization =
                    customizations.FirstOrDefault(
                        f =>
                            (f.Position.Equals(attribute) || f.Position.Equals("+" + attribute) ||
                             f.Position.Equals("-" + attribute)));
                if (customization == null) {
                    //no customization found, add the original field normally
                    resultDisplayables.Add(displayable);
                    continue;
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
                    resultDisplayables.Add(displayable);
                } else if (customization.Position.StartsWith("+")) {
                    if (!customization.Displayables.Any()) {
                        throw new MetadataException(
                            "right customizations must have a body, check your metadata at {0} | customization: {1}".Fmt(
                                overridenSchema, customization.Position));
                    }
                    resultDisplayables.Add(displayable);
                    var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables,
                        overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                    resultDisplayables.AddRange(resolvedDisplayables);
                } else {
                    //exact match
                    //if empty this would replace the existing displayable
                    var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables,
                        overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                    resultDisplayables.AddRange(resolvedDisplayables);
                }
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
