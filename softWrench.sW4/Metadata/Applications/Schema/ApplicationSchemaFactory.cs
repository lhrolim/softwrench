using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.Reference;
using softWrench.sW4.Metadata.Stereotypes;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata.Applications.Schema {

    public static class ApplicationSchemaFactory {


        public static ApplicationSchemaDefinition GetSyncInstance(String applicationName, string idFieldName) {

            var syncDisplayables = new List<IApplicationDisplayable>();

            var definition = new ApplicationSchemaDefinition(applicationName, "", ApplicationMetadataConstants.SyncSchema, SchemaStereotype.None, SchemaMode.None,
                ClientPlatform.Mobile, false, syncDisplayables, null, null, null, null, idFieldName, null);
            definition.FkLazyFieldsResolver = ApplicationSchemaLazyFkHandler.SyncSchemaLazyFkResolverDelegate;
            definition.ComponentDisplayableResolver = ReferenceHandler.ComponentDisplayableResolver;
            return definition;
        }

        public static ApplicationSchemaDefinition GetInstance(
          String applicationName, string title, string schemaId, SchemaStereotype stereotype,
          SchemaMode? mode, ClientPlatform? platform, bool @abstract,
          [NotNull] List<IApplicationDisplayable> displayables, [NotNull]IDictionary<string, string> schemaProperties,
          ApplicationSchemaDefinition parentSchema, ApplicationSchemaDefinition printSchema, [NotNull] ApplicationCommandSchema commandSchema,
          string idFieldName, string unionSchema) {

            var schema = new ApplicationSchemaDefinition(applicationName, title, schemaId, stereotype, mode, platform,
                @abstract, displayables, schemaProperties, parentSchema, printSchema, commandSchema, idFieldName, unionSchema);

            if (schema.ParentSchema != null) {
                schema.Displayables = MergeParentSchemaDisplayables(schema, schema.ParentSchema);
                schema.Mode = schema.Mode == null || schema.Mode == SchemaMode.None ? schema.ParentSchema.Mode : schema.Mode;
                schema.Stereotype = schema.Stereotype == SchemaStereotype.None ? schema.ParentSchema.Stereotype : schema.Stereotype;
                MergeWithParentProperties(schema);
                MergeWithParentCommands(schema);
            }
            schema.Title = title ?? BuildDefaultTitle(schema);
            AddHiddenRequiredFields(schema);

            MergeWithStereotypeSchema(schema);

            
            schema.FkLazyFieldsResolver = ApplicationSchemaLazyFkHandler.LazyFkResolverDelegate;
            schema.ComponentDisplayableResolver = ReferenceHandler.ComponentDisplayableResolver;
            
            return schema;
        }

        private static void MergeWithParentProperties(ApplicationSchemaDefinition schema) {
            foreach (var parentProperty in schema.ParentSchema.Properties) {
                if (!schema.Properties.ContainsKey(parentProperty.Key)) {
                    schema.Properties.Add(parentProperty);
                }
            }
        }

        private static void MergeWithParentCommands(ApplicationSchemaDefinition schema) {
            foreach (var parentCommand in schema.ParentSchema.CommandSchema.Commands) {
                if (!schema.CommandSchema.Commands.Contains(parentCommand)) {
                    schema.CommandSchema.Commands.Add(parentCommand);
                }
            }
        }

        private static List<IApplicationDisplayable> MergeParentSchemaDisplayables(ApplicationSchemaDefinition childSchema, IApplicationDisplayableContainer parentContainer) {
            var resultingDisplayables = new List<IApplicationDisplayable>();
            var parentDisplayables = parentContainer.Displayables;
            var childSections = DisplayableUtil.GetDisplayable<ApplicationSection>(typeof(ApplicationSection), childSchema.Displayables);
            foreach (var dis in parentDisplayables) {
                var parentSection = dis as ApplicationSection;
                if (parentSection == null) {
                    //just adding the non-section displayable, on the child schema
                    resultingDisplayables.Add(dis);
                    continue;
                }

                var concreteSection = childSections.FirstOrDefault(s => s.Id == parentSection.Id);
                if (concreteSection == null) {
                    //put the abstract anyway so that eventual subclasses of this can use it as well
                    var cloneable = dis as IPCLCloneable;
                    var clonedSection =(ApplicationSection) cloneable.Clone();
                    var resultDisplayables =MergeParentSchemaDisplayables(childSchema, clonedSection);
                    clonedSection.Displayables = resultDisplayables;
                    resultingDisplayables.Add(clonedSection);
                } else {
                    if (concreteSection.OrientationEnum == ApplicationSectionOrientation.horizontal) {
                        resultingDisplayables.Add(concreteSection);
                    } else if (concreteSection.OrientationEnum == ApplicationSectionOrientation.vertical) {
                        foreach (var sectionDisplayable in concreteSection.Displayables) {
                            resultingDisplayables.Add(sectionDisplayable);
                        }
                    }

                }

            }
            return resultingDisplayables;
        }

        private static void MergeWithStereotypeSchema(ApplicationSchemaDefinition schema) {
            var stereotypeProvider = StereotypeFactory.LookupStereotype(schema.Stereotype, schema.Mode);
            var stereotypeProperties = stereotypeProvider.StereotypeProperties();

            foreach (var stereotypeProperty in stereotypeProperties) {
                string key = stereotypeProperty.Key;
                if (!schema.Properties.ContainsKey(key)) {
                    schema.Properties.Add(key, stereotypeProperty.Value);
                }
            }
        }

        private static string BuildDefaultTitle(ApplicationSchemaDefinition schema) {
            var name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(schema.ApplicationName.ToLower());
            switch (schema.Stereotype) {
                case SchemaStereotype.List:
                    return name + " Grid";
                case SchemaStereotype.Detail:
                    return name + " Detail";
            }
            return null;
        }

        /// <summary>
        /// Add PK and FK fields, as hidden ones, if they weren´t already provided in the metadata.xml configuration. 
        /// This avoid common mistakes.
        /// </summary>
        private static void AddHiddenRequiredFields(ApplicationSchemaDefinition schema) {
            var idFieldDefinition = schema.Fields.FirstOrDefault(f => f.Attribute == schema.IdFieldName);
            if (idFieldDefinition == null && !schema.Abstract) {
                //if its abstract, it may be declared on the child schemas, so we wont take any action
                var idField = ApplicationFieldDefinition.HiddenInstance(schema.ApplicationName, schema.IdFieldName);
                schema.Displayables.Add(idField);
            }
            CreateMissingRelationshipFields(schema);
        }

        private static void CreateMissingRelationshipFields(ApplicationSchemaDefinition schema) {
            foreach (var dataProviderContainer in schema.DataProviderContainers) {
                if (schema.Fields.All(f => f.Attribute != dataProviderContainer.Target)) {
                    //adding hidden field to the from clause, in the case it wasn´t explicity declared in the metadata.xml
                    var fromField = ApplicationFieldDefinition.HiddenInstance(schema.ApplicationName, dataProviderContainer.Target);
                    schema.Displayables.Add(fromField);
                }
            }
        }

        private static List<IApplicationDisplayable> OnApplySecurityPolicy(ApplicationSchemaDefinition schema, IEnumerable<Role> userRoles) {
            var activeFieldRoles = RoleManager.ActiveFieldRoles();
            if (activeFieldRoles == null || activeFieldRoles.Count == 0) {
                return schema.Displayables;
            }
            var resultingFields = new List<IApplicationDisplayable>();
            foreach (var field in schema.Displayables) {
                if (!activeFieldRoles.Contains(field.Role)) {
                    resultingFields.Add(field);
                } else {
                    var enumerable = userRoles as IList<Role> ?? userRoles.ToList();
                    if (enumerable.Any(r => r.Name == field.Role)) {
                        resultingFields.Add(field);
                    }
                }
            }
            return resultingFields;
        }




        private static ApplicationSchemaDefinition OnApplyPlatformPolicy(ApplicationSchemaDefinition schema, ClientPlatform platform, List<IApplicationDisplayable> displayables)
        {
            //pass null on ParentSchema to avoid reMerging the parentSchemaData
            var resultSchema = GetInstance(schema.ApplicationName, schema.Title, schema.SchemaId, schema.Stereotype, schema.Mode, platform, schema.Abstract, displayables,
                schema.Properties, null, schema.PrintSchema, schema.CommandSchema, schema.IdFieldName, schema.UnionSchema);
            resultSchema.FieldWhichHaveDeps = schema.FieldWhichHaveDeps;
            resultSchema.DependantFields = schema.DependantFields;
            return resultSchema;
        }

        //        protected abstract ApplicationSchema OnApplyPlatformPolicy(ClientPlatform platform, IList<IApplicationDisplayable> fields);

        [NotNull]
        public static ApplicationSchemaDefinition ApplyPolicy(this ApplicationSchemaDefinition schema, [NotNull] IEnumerable<Role> userRoles, ClientPlatform platform) {
            if (userRoles == null) throw new ArgumentNullException("userRoles");

            return OnApplyPlatformPolicy(schema, platform, OnApplySecurityPolicy(schema, userRoles));
        }

        [NotNull]
        public static IEnumerable<ApplicationFieldDefinition> RelationshipFields(this ApplicationSchemaDefinition schema) {
            return schema.RelationshipFields;
        }


        [NotNull]
        public static IEnumerable<ApplicationFieldDefinition> NonRelationshipFields(this ApplicationSchemaDefinition schema) {
            return schema.Fields.Where(f => !f.Attribute.Contains(".") && !f.Attribute.Contains("#"));
        }
    }

}

