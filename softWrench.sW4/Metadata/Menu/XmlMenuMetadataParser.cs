using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using cts.commons.Util;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sw4.Shared2.Metadata.Modules;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata.Menu {

    class XmlMenuMetadataParser {

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public MenuDefinition Parse([NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException("stream");

            var modules = new List<ModuleDefinition>();
            var document = XDocument.Load(stream);
            var menuElement = document.Root;
            if (null == menuElement) throw new InvalidDataException();

            var displacement = menuElement.Attribute(XmlMenuMetadataSchema.MenuDisplacementAttribute).ValueOrDefault("vertical");
            var indexItem = menuElement.Attribute(XmlMenuMetadataSchema.MenuIndexItemAttribute).ValueOrDefault((string)null);
            var childs = menuElement.Elements();
            var leafs = new List<MenuBaseDefinition>();
            foreach (var xElement in childs) {
                var xName = xElement.Name.LocalName;
                if (xName == XmlMenuMetadataSchema.ContainerElement) {
                    MenuBaseDefinition newIndexLeaf;
                    leafs.Add(ParseContainer(xElement, indexItem, out newIndexLeaf, modules));
                    continue;
                }
                var leaf = BuildLeaf(xName, xElement, modules);
                leafs.Add(leaf);
            }
            var menuDefinition = new MenuDefinition(leafs, displacement, indexItem);
            modules.Sort();
            menuDefinition.Modules = modules;
            return menuDefinition;
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
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            return new ResourceMenuItem(id, role, path, @params, tooltip, moduleName);
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
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            var parameters =
                xElement.Attribute(XmlMenuMetadataSchema.ApplicationMenuParametersAttribute).ValueOrDefault((string)null);
            SchemaMode mode;
            Enum.TryParse(modeAttr, true, out mode);
            return new ApplicationMenuItemDefinition(id, title, role, tooltip, icon, application, schema, mode, PropertyUtil.ConvertToDictionary(parameters), moduleName);
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
            var moduleName = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = xElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }
            var parameters =
                xElement.Attribute(XmlMenuMetadataSchema.ActionMenuParametersAttribute).ValueOrDefault((string)null);
            var controller = xElement.Attribute(XmlMenuMetadataSchema.ActionMenuControllerAttribute).Value;
            return new ActionMenuItemDefinition(id, title, role, tooltip, icon, action, controller, target, PropertyUtil.ConvertToDictionary(parameters), moduleName);
        }

        [NotNull]
        private static MenuBaseDefinition ParseDivider(XElement xElement) {
            var id = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = xElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            return new DividerMenuItem(id, title, role, tooltip, icon);
        }

        [NotNull]
        private MenuContainerDefinition ParseContainer(XElement containerElement, string indexItem, out MenuBaseDefinition indexLeaf,
            List<ModuleDefinition> modules) {
            var id = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseIdAttribute).ValueOrDefault((string)null);
            var title = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseTitleAttribute).ValueOrDefault((string)null);
            var tooltip = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseTipAttribute).ValueOrDefault((string)null);
            var icon = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseIconAttribute).ValueOrDefault((string)null);
            var role = containerElement.Attribute(XmlMenuMetadataSchema.MenuBaseRoleAttribute).ValueOrDefault((string)null);
            var moduleName = containerElement.Attribute(XmlMenuMetadataSchema.ContainerModuleName).ValueOrDefault((string)null);
            var moduleAlias = containerElement.Attribute(XmlMenuMetadataSchema.ContainerModuleAlias).ValueOrDefault((string)null);
            var controller = containerElement.Attribute(XmlMenuMetadataSchema.ActionMenuControllerAttribute).ValueOrDefault((string)null);
            var action = containerElement.Attribute(XmlMenuMetadataSchema.ActionMenuActionAttribute).ValueOrDefault((string)null);
            if (moduleName != null) {
                modules.Add(new ModuleDefinition(moduleName, moduleAlias));
            }

            var leafs = new List<MenuBaseDefinition>();
            indexLeaf = null;
            foreach (var xElement in containerElement.Elements()) {
                var xName = xElement.Name.LocalName;
                if (xName == XmlMenuMetadataSchema.ContainerElement) {
                    leafs.Add(ParseContainer(xElement, indexItem, out indexLeaf, modules));
                    continue;
                }
                var leaf = BuildLeaf(xName, xElement, modules);
                if (indexItem != null && indexLeaf == null && leaf != null && leaf.Id == indexItem) {
                    indexLeaf = leaf;
                }
                leafs.Add(leaf);
            }
            return new MenuContainerDefinition(id, title, role, tooltip, icon, moduleName, controller, action, leafs);
        }

        [CanBeNull]
        private static MenuBaseDefinition BuildLeaf(string xName, XElement xElement, List<ModuleDefinition> modules) {
            switch (xName) {
                case XmlMenuMetadataSchema.ActionElement:
                    return ParseAction(xElement, modules);
                case XmlMenuMetadataSchema.ApplicationElement:
                    return ParseApplication(xElement, modules);
                case XmlMenuMetadataSchema.ResourceRefElement:
                    return ParseResourceRef(xElement, modules);
                case XmlMenuMetadataSchema.DividerElement:
                    return ParseDivider(xElement);
            }
            return null;
        }

    }
}
