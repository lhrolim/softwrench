using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Security {
    class ApplicationFieldSecurityApplier {


        public static List<IApplicationDisplayable> OnApplySecurityPolicy(ApplicationSchemaDefinition schema,IEnumerable<Role> userRoles, string schemaFieldsToDisplay, MergedUserProfile profile) {
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

            resultingFields.AddRange(GetAllowedFields(fieldsToRetain, schema.Displayables,
                applicationPermission.ContainerPermissions.Where(c=> c.Schema.EqualsIc(schema.SchemaId)), "main"));

            return resultingFields;

        }


        private static List<IApplicationDisplayable> GetAllowedFields(ISet<string> fieldsToRetain,
            IEnumerable<IApplicationDisplayable> displayables,
            IEnumerable<ContainerPermission> containerPermissions, string currentContainerKey) {
            var permissions = containerPermissions as IList<ContainerPermission> ?? containerPermissions.ToList();
            var container = permissions.FirstOrDefault(c => c.ContainerKey.EqualsIc(currentContainerKey));

            var resultingFields = new List<IApplicationDisplayable>();

            foreach (var field in displayables) {
                if (field is ApplicationTabDefinition) {
                    //tabs change the ApplicationContainer
                    var tab = (ApplicationTabDefinition)field;
                    tab.Displayables = GetAllowedFields(fieldsToRetain, tab.Displayables, permissions, tab.TabId);
                    if (tab.Displayables.Any()) {
                        //if at least one field remained, let´s keep the tab
                        resultingFields.Add(tab);
                    }
                } else {
                    if (field is ApplicationSection) {
                        var section = (ApplicationSection)field;
                        var numberOfFieldsBefore = section.Displayables.Count;
                        section.Displayables = GetAllowedFields(fieldsToRetain, section.Displayables, permissions,currentContainerKey);
                        if (section.Displayables.Any() && numberOfFieldsBefore !=0) {
                            //if at least one field remained, let´s keep the section, otherwise it would be discarded as well
                            //unless ít´s a section to import stuf
                            resultingFields.Add(section);
                        }
                    } else {
                        var isFieldAllowed = DoApplyFieldPermission(fieldsToRetain, field, container);
                        if (isFieldAllowed) {
                            resultingFields.Add(field);
                        }
                    }
                }
            }
            return resultingFields;

        }

        private static bool DoApplyFieldPermission(ISet<string> fieldsToRetain, IApplicationDisplayable field,
            ContainerPermission container) {
            var appDisplayable = field as IApplicationIndentifiedDisplayable;
            if (appDisplayable == null) {
                //no way to infer on that field
                return true;
            }

            var isRetained = fieldsToRetain.Any() && !fieldsToRetain.Any(f => appDisplayable.Attribute.EqualsIc(f));
            if (isRetained) {
                //move on, this field should be discarded
                return false;
            }

            if (container == null) {
                //no security applied, adding the field
                return true;
            }

            var fieldPermission =
                container.FieldPermissions.FirstOrDefault(f => f.FieldKey.EqualsIc(appDisplayable.Role));
            if (fieldPermission == null || fieldPermission.Permission.EqualsIc("fullcontrol")) {
                //no security applied, adding the field
                return true;
            }
            if (fieldPermission.Permission.EqualsIc("none")) {
                return false;
            }
            if (fieldPermission.Permission.EqualsIc("readonly")) {
                appDisplayable.IsReadOnly = true;
                return true;
            }
            return true;
        }
    }
}
