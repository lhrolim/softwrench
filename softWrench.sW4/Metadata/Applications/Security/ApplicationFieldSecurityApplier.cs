﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using log4net;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Security {
    class ApplicationFieldSecurityApplier {

        private static ILog Log = LogManager.GetLogger(typeof(ApplicationFieldSecurityApplier));


        public static List<IApplicationDisplayable> OnApplySecurityPolicy(ApplicationSchemaDefinition schema, IEnumerable<Role> userRoles, string schemaFieldsToDisplay, MergedUserProfile profile) {
            var applicationPermission = profile.GetPermissionByApplication(schema.ApplicationName);

            if (schemaFieldsToDisplay == null) {
                if (applicationPermission == null || applicationPermission.ContainerPermissions == null || !applicationPermission.ContainerPermissions.Any(c => c.Schema.EqualsIc(schema.SchemaId))) {
                    //no restriction at all
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

            var containerPermissions = applicationPermission.ContainerPermissions.Where(c => c.Schema.EqualsIc(schema.SchemaId));
            var compositionPermissions = applicationPermission.CompositionPermissions == null ? new List<CompositionPermission>()
                : applicationPermission.CompositionPermissions.Where(c => c.Schema.EqualsIc(schema.SchemaId));
            resultingFields.AddRange(GetAllowedFields(fieldsToRetain, schema.Displayables, compositionPermissions,
                containerPermissions, "main"));

            return resultingFields;

        }


        private static List<IApplicationDisplayable> GetAllowedFields(ISet<string> fieldsToRetain,
            IEnumerable<IApplicationDisplayable> displayables, IEnumerable<CompositionPermission> compositionPermissions,
            IEnumerable<ContainerPermission> containerPermissions, string currentContainerKey) {
            var permissions = containerPermissions as IList<ContainerPermission> ?? containerPermissions.ToList();
            var container = permissions.FirstOrDefault(c => c.ContainerKey.EqualsIc(currentContainerKey));

            var resultingFields = new List<IApplicationDisplayable>();

            foreach (var field in displayables) {
                if (field is ApplicationTabDefinition) {
                    //tabs change the ApplicationContainer
                    var tab = (ApplicationTabDefinition)field;
                    tab.Displayables = GetAllowedFields(fieldsToRetain, tab.Displayables, compositionPermissions, permissions, tab.TabId);
                    if (tab.Displayables.Any()) {
                        //if at least one field remained, let´s keep the tab
                        resultingFields.Add(tab);
                    }
                } else {
                    if (field is ApplicationCompositionDefinition) {
                        ApplicationCompositionDefinition comp = (ApplicationCompositionDefinition)field;
                        var compPermission = compositionPermissions.FirstOrDefault(c => (c.CompositionKey + "_").EqualsIc(comp.TabId));
                        if (compPermission == null) {
                            resultingFields.Add(comp);
                        } else if (compPermission.HasNoPermission) {
                            continue;
                        } else if (comp.Schema is ApplicationCompositionCollectionSchema) {
                            var cloned = (ApplicationCompositionDefinition)comp.Clone();
                            var collSchema = (ApplicationCompositionCollectionSchema)cloned.Schema;
                            //these allowXXX can be expressions that would need to be evaluated at client side too, depending on the datamap
                            //hence we can only mark them as false if the permissions are not granted, but we need to keep them intact otherwise
                            if (!compPermission.AllowUpdate) {
                                collSchema.CollectionProperties.AllowUpdate = "false";
                            }
                            if (!compPermission.AllowCreation) {
                                collSchema.CollectionProperties.AllowInsertion= "false";
                            }

                            if (!compPermission.AllowRemoval) {
                                collSchema.CollectionProperties.AllowRemoval = "false";
                            }


                            collSchema.CollectionProperties.AllowRemoval = compPermission.AllowRemoval.ToString().ToLower();
                            resultingFields.Add(cloned);
                        }
                    } else if (field is ApplicationSection) {
                        var section = (ApplicationSection)field;
                        var numberOfFieldsBefore = section.Displayables.Count;
                        section.Displayables = GetAllowedFields(fieldsToRetain, section.Displayables, compositionPermissions, permissions, currentContainerKey);
                        if (section.Displayables.Any() && numberOfFieldsBefore != 0) {
                            //if at least one field remained, let´s keep the section, otherwise it would be discarded as well
                            //unless ít´s a section to import stuf
                            resultingFields.Add(section);
                        }
                    } else {
                        var result = DoApplyFieldPermission(fieldsToRetain, field, container);
                        if (FieldPermission.FullControl.Equals(result)) {
                            resultingFields.Add(field);
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
            FullControl, ReadOnly, None
        }


        private static FieldPermission DoApplyFieldPermission(ISet<string> fieldsToRetain, IApplicationDisplayable field,
            ContainerPermission container) {
            var appDisplayable = field as IApplicationIndentifiedDisplayable;
            if (appDisplayable == null) {
                //no way to infer on that field
                return FieldPermission.FullControl;
            }

            var isRetained = fieldsToRetain.Any() && !fieldsToRetain.Any(f => appDisplayable.Attribute.EqualsIc(f));
            if (isRetained) {
                //move on, this field should be discarded
                return FieldPermission.None;
            }

            if (container == null) {
                //no security applied, adding the field
                return FieldPermission.FullControl;
            }

            var fieldPermission =
                container.FieldPermissions.FirstOrDefault(f => f.FieldKey.EqualsIc(appDisplayable.Role));
            if (fieldPermission == null || fieldPermission.Permission.EqualsIc("fullcontrol")) {
                //no security applied, adding the field
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
