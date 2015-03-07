﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Exceptions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata.Validator {
    class SchemaMerger {
        private const string NonCustomizableFound = "overriden schemas can only contain customizations, however found {0} wrong displayables ( {1}) for schema {2}";

        private static ILog Log = LogManager.GetLogger(typeof (SchemaMerger));

        public static void MergeSchemas(ApplicationSchemaDefinition original, ApplicationSchemaDefinition overridenSchema,IEnumerable<DisplayableComponent> components) {
            var resultDisplayables = new List<IApplicationDisplayable>();
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
            var customizations = DisplayableUtil.GetDisplayable<ApplicationSchemaCustomization>(typeof(ApplicationSchemaCustomization), overridenSchema.Displayables);

            var fieldsThatShouldBeCustomized = customizations.Count();
            var customizationsActuallyApplied = new HashSet<int>();

            foreach (var displayable in original.Displayables) {
                var attrDisplayablee = displayable as IApplicationIndentifiedDisplayable;
                if (attrDisplayablee == null) {
                    //can only replace fields that have attributes
                    resultDisplayables.Add(displayable);
                    continue;
                }
                var attribute = attrDisplayablee.Attribute;

                var customization = customizations.FirstOrDefault(f => (f.Position.Equals(attribute) || f.Position.Equals("+" + attribute) || f.Position.Equals("-" + attribute)));
                if (customization == null) {
                    //no customization found, add the original field normally
                    resultDisplayables.Add(displayable);
                    continue;
                }
                Log.DebugFormat("applying customization {0} on schema {1}",customization.Position,overridenSchema);
                customizationsActuallyApplied.Add(customizations.IndexOf(customization));

                if (customization.Position.StartsWith("-")) {
                    if (!customization.Displayables.Any()) {
                        throw new MetadataException("left customizations must have a body, check your metadata at {0}  | customization: {1}".Fmt(overridenSchema, customization.Position));
                    }
                    var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables, overridenSchema, overridenSchema.ComponentDisplayableResolver,components);
                    resultDisplayables.AddRange(resolvedDisplayables);
                    resultDisplayables.Add(displayable);
                } else if (customization.Position.StartsWith("+")) {
                    if (!customization.Displayables.Any()) {
                        throw new MetadataException("right customizations must have a body, check your metadata at {0} | customization: {1}".Fmt(overridenSchema, customization.Position));
                    }
                    resultDisplayables.Add(displayable);
                    var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables, overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                    resultDisplayables.AddRange(resolvedDisplayables);
                } else {
                    //exact match
                    //if empty this would replace the existing displayable
                    var resolvedDisplayables = DisplayableUtil.PerformReferenceReplacement(customization.Displayables, overridenSchema, overridenSchema.ComponentDisplayableResolver, components);
                    resultDisplayables.AddRange(resolvedDisplayables);
                }

            }
            if (customizationsActuallyApplied.Count != fieldsThatShouldBeCustomized) {
                var names = new List<string>();
                for (var i = 0; i < customizations.Count; i++) {
                    if (!customizationsActuallyApplied.Contains(i)) {
                        names.Add(customizations[i].Position);
                    }
                }
                throw new MetadataException("customizations {0} could not be applied cause the corresponding fields were not found in schema {1}".Fmt(String.Join(",", names), overridenSchema));
            }
            original.Displayables = resultDisplayables;
        }

        private static IApplicationDisplayable[] GetNonCustomizableFields(ApplicationSchemaDefinition overridenSchema) {
            var result = overridenSchema.Displayables.Where(disp => !(disp is ApplicationSchemaCustomization)).Where(disp => !(disp is ApplicationFieldDefinition) || !((ApplicationFieldDefinition)disp).AutoGenerated).ToList();
            return result.ToArray();
        }
    }
}
