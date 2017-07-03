using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using log4net;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using Role = softwrench.sw4.user.classes.entities.Role;

namespace softWrench.sW4.Metadata.Applications.Security {
    class ApplicationFieldSecurityApplier {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationFieldSecurityApplier));


        public static List<IApplicationDisplayable> OnApplySecurityPolicy(ApplicationSchemaDefinition schema, IEnumerable<Role> userRoles, string schemaFieldsToDisplay, MergedUserProfile profile) {
            var user = SecurityFacade.CurrentUser();
            if (user.IsSwAdmin()) {
                return schema.Displayables;
            }

            var applicationPermission = profile.GetPermissionByApplication(schema.ApplicationName);

            if (schemaFieldsToDisplay == null) {
                if (applicationPermission == null || (!SchemaRequiresCompositionValidation(schema, applicationPermission) && (!applicationPermission.HasContainerPermissionOfSchema(schema.SchemaId)))) {
                    //no restriction at all. If AllowUpdate isn´t granted, though, we need to modify the compositions
                    Log.DebugFormat("schema {0} requires no further validation, returning all fields", schema.SchemaId);
                    return schema.Displayables;
                }
            }

            var fieldsToRetain = new HashSet<string>();

            if (schemaFieldsToDisplay != null) {
                fieldsToRetain.AddAll(schemaFieldsToDisplay.Split(','));
            }
            var resultingFields = new List<IApplicationDisplayable>();


            //            DisplayableUtil.GetDisplayable<IApplicationDisplayable>(typeof(IApplicationDisplayable),
            //                schema.Displayables);

            if (schemaFieldsToDisplay != null && applicationPermission == null) {
                //dashboard scenario
                applicationPermission = new ApplicationPermission();
                applicationPermission.ContainerPermissions = new LinkedHashSet<ContainerPermission>();
            }


            var containerPermissions = applicationPermission.ContainerPermissions?.Where(c => c.Schema.EqualsIc(schema.SchemaId)) ?? new HashSet<ContainerPermission>();

            var compositionPermissions = applicationPermission.CompositionPermissions?.Where(c => c.Schema.EqualsIc(schema.SchemaId)) ?? new List<CompositionPermission>();

            resultingFields.AddRange(GetAllowedFields(applicationPermission, fieldsToRetain, schema.Displayables, compositionPermissions,
                containerPermissions, "main"));

            return resultingFields;

        }

        private static bool SchemaRequiresCompositionValidation(ApplicationSchemaDefinition schema, ApplicationPermission applicationPermission) {
            if (schema.Stereotype.Equals(SchemaStereotype.List) || schema.Stereotype.Equals(SchemaStereotype.CompositionList)) {
                return false;
            }

            return !applicationPermission.AllowUpdate;
        }


        private static List<IApplicationDisplayable> GetAllowedFields(ApplicationPermission applicationPermission, System.Collections.Generic.ISet<string> fieldsToRetain, IEnumerable<IApplicationDisplayable> displayables,
            IEnumerable<CompositionPermission> compositionPermissions, IEnumerable<ContainerPermission> containerPermissions, string currentContainerKey) {
            var permissions = containerPermissions as IList<ContainerPermission> ?? containerPermissions.ToList();
            var container = permissions.FirstOrDefault(c => c.ContainerKey.EqualsIc(currentContainerKey));

            var resultingFields = new List<IApplicationDisplayable>();

            foreach (var field in displayables) {
                if (field is ApplicationTabDefinition) {
                    //tabs change the ApplicationContainer
                    var tab = (ApplicationTabDefinition)field;
                    var tabPermission = permissions.FirstOrDefault(p => p.ContainerKey.EqualsIc(tab.TabId));
                    tab.Displayables = GetAllowedFields(applicationPermission,
                        fieldsToRetain, tab.Displayables, compositionPermissions, permissions, tab.TabId);
                    if (tab.Displayables.Any() && (tabPermission == null || tabPermission.AllowView)) {
                        //if at least one field remained and the tab is not set to be hidden, let´s keep the tab
                        resultingFields.Add(tab);
                    }
                } else {
                    if (field is ApplicationCompositionDefinition) {
                        var comp = (ApplicationCompositionDefinition)field;


                        var compPermission = compositionPermissions.FirstOrDefault(c => EntityUtil.IsRelationshipNameEquals(c.CompositionKey, comp.TabId));

                        if (applicationPermission.AllowUpdate) {
                            if (compPermission == null) {
                                //no specific composition permission, and application update allowed --> add composition with no modifications
                                resultingFields.Add(comp);
                                continue;
                            }
                        } else {
                            // if top level application update is not granted, compositions should not be granted either
                            compPermission = new CompositionPermission() {
                                AllowUpdate = false,
                                AllowCreation = false,
                                AllowRemoval = false,
                                AllowView = applicationPermission.AllowView
                            };
                        }

                        if (compPermission.HasNoPermission) {
                            //excluding the composition entirely
                            continue;
                        } else if (comp.Schema is ApplicationCompositionCollectionSchema) {
                            var cloned = (ApplicationCompositionDefinition)comp.Clone();
                            var collSchema = (ApplicationCompositionCollectionSchema)cloned.Schema;
                            //these allowXXX can be expressions that would need to be evaluated at client side too (that´s the reason of string instead of boolean),
                            //hence we can only mark them as false if the permissions are not granted, but we need to keep them intact otherwise
                            //the application top-level update permission has to be granted however
                            if (!compPermission.AllowUpdate) {
                                collSchema.CollectionProperties.AllowUpdate = "false";
                            }
                            if (!compPermission.AllowCreation) {
                                collSchema.CollectionProperties.AllowInsertion = "false";
                            }
                            if (!compPermission.AllowRemoval) {
                                collSchema.CollectionProperties.AllowRemoval = "false";
                            }
                            resultingFields.Add(cloned);
                        }
                    } else if (field is ApplicationSection) {
                        var section = (ApplicationSection)field;

                        if (section.Id != null && container!=null) {
                            var sectionPermission = container.SectionPermissions.FirstOrDefault(s => s.SectionId.EqualsIc(section.Id));
                            if (sectionPermission != null && !sectionPermission.AnyPermission) {
                                continue;
                            }
                            if (sectionPermission != null && sectionPermission.ReadOnly) {
                                section.EnableExpression = "false";
                            }
                        }


                        var numberOfFieldsBefore = section.Displayables.Count;
                        section.Displayables = GetAllowedFields(applicationPermission,
                            fieldsToRetain, section.Displayables, compositionPermissions, permissions, currentContainerKey);
                        if (section.Displayables.Any() && numberOfFieldsBefore != 0) {
                            //if at least one field remained, let´s keep the section, otherwise it would be discarded as well
                            //unless ít´s a section to import stuf
                            resultingFields.Add(section);
                        }
                    } else {
                        var result = DoApplyFieldPermission(fieldsToRetain, field, container);
                        if (FieldPermission.Ignore.Equals(result)) {
                            // just ignores security and adds the field
                            resultingFields.Add(field);
                        } else if (FieldPermission.FullControl.Equals(result)) {
                            var ass = field as ApplicationAssociationDefinition;
                            if (field.IsReadOnly) {
                                Log.InfoFormat("adding cloned non-readonly version of field {0}", field.Role);
                                var clone = (IApplicationDisplayable)((IPCLCloneable)field).Clone();
                                clone.IsReadOnly = false;
                                resultingFields.Add(clone);
                            } else if (ass != null && "false".Equals(ass.EnableExpression)) {
                                Log.InfoFormat("adding cloned non-readonly version of field {0}", field.Role);
                                var clone = (ApplicationAssociationDefinition)((IPCLCloneable)field).Clone();
                                clone.IsReadOnly = false;
                                clone.EnableExpression = "true";
                                resultingFields.Add(clone);
                            } else {
                                resultingFields.Add(field);
                            }
                        } else if (FieldPermission.ReadOnly.Equals(result) && field is IPCLCloneable) {
                            Log.InfoFormat("adding cloned readonly version of field {0}", field.Role);
                            var clone = (IApplicationDisplayable)((IPCLCloneable)field).Clone();
                            clone.IsReadOnly = true;
                            resultingFields.Add(clone);
                        }
                    }
                }
            }
            return resultingFields;

        }

        private enum FieldPermission {
            FullControl, ReadOnly, None, Ignore
        }


        private static FieldPermission DoApplyFieldPermission(System.Collections.Generic.ISet<string> fieldsToRetain, IApplicationDisplayable field,
            ContainerPermission container) {
            var appDisplayable = field as IApplicationIndentifiedDisplayable;
            if (appDisplayable == null) {
                //no way to infer on that field
                return FieldPermission.Ignore;
            }

            var isRetained = fieldsToRetain.Any() && !fieldsToRetain.Any(f => appDisplayable.Attribute.EqualsIc(f));
            if (isRetained) {
                //move on, this field should be discarded
                return FieldPermission.None;
            }

            if (container == null) {
                //no security applied, adding the field
                return FieldPermission.Ignore;
            }

            var fieldPermission =
                container.FieldPermissions.FirstOrDefault(f => f.FieldKey.EqualsIc(appDisplayable.Role));
            if (fieldPermission == null) {
                //no security applied, adding the field
                return FieldPermission.Ignore;
            }
            if (fieldPermission.Permission.EqualsIc("fullcontrol")) {
                return FieldPermission.FullControl;
            }
            if (fieldPermission.Permission.EqualsIc("none")) {
                return FieldPermission.None;
            }
            if (fieldPermission.Permission.EqualsIc("readonly")) {
                return FieldPermission.ReadOnly;
            }
            return FieldPermission.FullControl;
        }
    }
}
