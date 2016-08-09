using System;
using System.Collections.Generic;
using System.Xml.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata.Menu;
using softwrench.sw4.Shared2.Metadata.Modules;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata.Parsing;

namespace softWrench.sW4.Metadata.Menu {
    internal class XmlMenuParserCommons {


        internal static List<MenuBaseDefinition> BuildLeafs(XElement menuElement, string indexItem, List<ModuleDefinition> modules = null) {
            if (modules == null) {
                modules = new List<ModuleDefinition>();
            }

            var childs = menuElement.Elements();
            var leafs = new List<MenuBaseDefinition>();
            foreach (var xElement in childs) {
                var xName = xElement.Name.LocalName;
                if (xName == XmlMenuMetadataSchema.ContainerElement || xName == XmlMenuMetadataSchema.ReferenceElement) {
                    MenuBaseDefinition newIndexLeaf;
                    leafs.Add(ParseContainer(xElement, indexItem, out newIndexLeaf, modules, xName == XmlMenuMetadataSchema.ContainerElement));
                    continue;
                }

                var leaf = BuildLeaf(xName, xElement, modules);
                leafs.Add(leaf);
            }
            return leafs;
        }


        [NotNull]
        private static MenuBaseDefinition ParseResourceRef(XElement xElement, ICollection<ModuleDefinition> modules) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var @params = xElement.Attribute(XmlMenuMetadataSchema.ResourceMenuParamsAttribute).ValueOrDefault((string)null);
            var path = xElement.Attribute(XmlMenuMetadataSchema.ResourceMenuPathAttribute).Value;
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var moduleName = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            var customizationPosition = xElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            var permissionExpression = xElement.AttributeValue("permissionexpression");
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            return new ResourceMenuItem(id, role, path, @params, tooltip, moduleName, permissionExpression, customizationPosition);
        }

        [NotNull]
        private static MenuBaseDefinition ParseApplication(XElement xElement, ICollection<ModuleDefinition> modules) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var application = xElement.Attribute(XmlMenuMetadataSchema.ApplicationMenuRefAttribute).ValueOrDefault((string)null);
            var schema = xElement.Attribute(XmlMenuMetadataSchema.ApplicationMenuSchemaAttribute).ValueOrDefault("list");
            var modeAttr = xElement.Attribute(XmlMenuMetadataSchema.ApplicationMenuModeAttribute).ValueOrDefault((string)null);
            var moduleName = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var customizationPosition = xElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            var permissionExpression = xElement.AttributeValue("permissionexpression");
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            var parameters =
                xElement.Attribute(XmlMenuMetadataSchema.ApplicationMenuParametersAttribute).ValueOrDefault((string)null);
            SchemaMode mode;
            Enum.TryParse(modeAttr, true, out mode);
            return new ApplicationMenuItemDefinition(id, title, role, tooltip, icon, application, schema, mode, PropertyUtil.ConvertToDictionary(parameters), moduleName, permissionExpression, customizationPosition);
        }

        [NotNull]
        private static MenuBaseDefinition ParseService(XElement xElement, ICollection<ModuleDefinition> modules) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var customizationPosition = xElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);

            var moduleName = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }

            var service = xElement.Attribute(XmlMenuMetadataSchema.ServiceMenuServiceAttribute).Value;
            var method = xElement.Attribute(XmlMenuMetadataSchema.ServiceMenuMethodAttribute).Value;

            return new ServiceMenuItemDefinition(id, title, role, tooltip, icon, customizationPosition, service, method);
        }

        [NotNull]
        private static MenuBaseDefinition ParseAction(XElement xElement, ICollection<ModuleDefinition> modules) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var action = xElement.Attribute(XmlMenuMetadataSchema.ActionMenuActionAttribute).ValueOrDefault((string)null);
            var target = xElement.Attribute(XmlMenuMetadataSchema.ActionMenuTargetAttribute).ValueOrDefault((string)null);
            var customizationPosition = xElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            var moduleName = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            var permissionExpression = xElement.AttributeValue("permissionexpression");
            var parameters =
                xElement.Attribute(XmlMenuMetadataSchema.ActionMenuParametersAttribute).ValueOrDefault((string)null);
            var controller = xElement.Attribute(XmlMenuMetadataSchema.ActionMenuControllerAttribute).Value;
            return new ActionMenuItemDefinition(id, title, role, tooltip, icon, action, controller, target, PropertyUtil.ConvertToDictionary(parameters), moduleName, permissionExpression, customizationPosition);
        }

        private static ExternalLinkMenuItemDefinition ParseLink(XElement xElement, List<ModuleDefinition> modules) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var customizationPosition = xElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            var link = xElement.Attribute(XmlMenuMetadataSchema.LinkElement).Value;
            var permissionExpression = xElement.AttributeValue("permissionexpression");

            var moduleName = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            var parameters =
                xElement.Attribute(XmlMenuMetadataSchema.ActionMenuParametersAttribute).ValueOrDefault((string)null);
            return new ExternalLinkMenuItemDefinition(id, title, role, tooltip, icon, link, PropertyUtil.ConvertToDictionary(parameters), moduleName, permissionExpression, customizationPosition);
        }

        [NotNull]
        private static MenuBaseDefinition ParseDivider(XElement xElement) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var customizationPosition = xElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            return new DividerMenuItem(id, title, role, tooltip, icon, customizationPosition);
        }

        [NotNull]
        internal static MenuContainerDefinition ParseContainer(XElement containerElement, string indexItem, out MenuBaseDefinition indexLeaf,
            List<ModuleDefinition> modules, Boolean isContainer) {
            var id = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var moduleName = containerElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = containerElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            var controller = containerElement.Attribute(XmlMenuMetadataSchema.ActionMenuControllerAttribute).ValueOrDefault((string)null);
            var permissionExpression = containerElement.AttributeValue("permissionexpression");
            var action = containerElement.Attribute(XmlMenuMetadataSchema.ActionMenuActionAttribute).ValueOrDefault((string)null);
            var customizationPosition = containerElement.Attribute(XmlMenuMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            var hasMainAction = containerElement.Attribute(XmlMenuMetadataSchema.ContainerHasMainAction).ValueOrDefault(false);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }

            var leafs = new List<MenuBaseDefinition>();
            indexLeaf = null;
            foreach (var xElement in containerElement.Elements()) {
                var xName = xElement.Name.LocalName;
                if (xName == XmlMenuMetadataSchema.ContainerElement) {
                    leafs.Add(ParseContainer(xElement, indexItem, out indexLeaf, modules, isContainer));
                    continue;
                }
                var leaf = BuildLeaf(xName, xElement, modules);
                if (indexItem != null && indexLeaf == null && leaf != null && leaf.Id == indexItem) {
                    indexLeaf = leaf;
                }
                leafs.Add(leaf);
            }
            if (isContainer) {
                return new MenuContainerDefinition(id, title, role, tooltip, icon, moduleName, controller, action, hasMainAction, customizationPosition, permissionExpression,leafs);
            }
            if (id == null) {
                throw MenuMetadataException.MissingIdException();
            }

            return new ReferenceMenuItemDefinition(id, title, role, tooltip, icon, moduleName, controller, action, hasMainAction, permissionExpression,leafs);
        }

        [CanBeNull]
        internal static MenuBaseDefinition BuildLeaf(string xName, XElement xElement, List<ModuleDefinition> modules) {
            switch (xName) {
                case XmlMenuMetadataSchema.ActionElement:
                return ParseAction(xElement, modules);

                case XmlMenuMetadataSchema.ServiceElement:
                return ParseService(xElement, modules);

                case XmlMenuMetadataSchema.LinkElement:
                return ParseLink(xElement, modules);

                case XmlMenuMetadataSchema.ApplicationElement:
                return ParseApplication(xElement, modules);

                case XmlMenuMetadataSchema.ResourceRefElement:
                return ParseResourceRef(xElement, modules);

                case XmlMenuMetadataSchema.DividerElement:
                return ParseDivider(xElement);
            }
            return null;
        }

        private static ReferenceMenuItemDefinition ParseReference(XElement xElement, List<ModuleDefinition> modules) {
            throw new NotImplementedException();
        }
    }
}
