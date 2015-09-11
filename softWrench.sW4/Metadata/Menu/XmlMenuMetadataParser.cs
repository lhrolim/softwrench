using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata.Menu.Containers;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sw4.Shared2.Metadata.Modules;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Menu {

    class XmlMenuMetadataParser {
        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all entity metadata.
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public MenuDefinition Parse(MenuTemplateCatalog catalog, [NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException("stream");

            var modules = new List<ModuleDefinition>();
            var document = XDocument.Load(stream);
            var menuElement = document.Root;
            if (null == menuElement) throw new InvalidDataException();
            var displacement = menuElement.Attribute(XmlMenuMetadataSchema.MenuDisplacementAttribute).ValueOrDefault("horizontal");
            var indexItem = menuElement.Attribute(XmlMenuMetadataSchema.MenuIndexItemAttribute).ValueOrDefault((string)null);
            var leafs = XmlMenuParserCommons.BuildLeafs(menuElement, indexItem, modules);
            if (catalog != null) {
                leafs = MergeWithCatalog(catalog, leafs);
            }
            var menuDefinition = new MenuDefinition(leafs, displacement, indexItem);
            modules.Sort();
            menuDefinition.Modules = modules;
            return menuDefinition;
        }

        private List<MenuBaseDefinition> MergeWithCatalog(MenuTemplateCatalog catalog, List<MenuBaseDefinition> leafs) {
            var mergedMenu = new List<MenuBaseDefinition>();
            foreach (var leaf in leafs) {
                if (leaf is ReferenceMenuItemDefinition) {
                    mergedMenu.Add(GetMergedDefinition((ReferenceMenuItemDefinition)leaf, catalog));
                } else {
                    mergedMenu.Add(leaf);
                }
            }

            return mergedMenu;
        }

        private MenuBaseDefinition GetMergedDefinition(ReferenceMenuItemDefinition referenceLeaf, MenuTemplateCatalog catalog) {
            var item = catalog.Leafs.FirstOrDefault(f => f.Id.EqualsIc(referenceLeaf.Id));
            if (item == null) {
                throw MenuMetadataException.MissingReferenceException(referenceLeaf.Id);
            }

            var clonedItem = MergeOwnProperties(referenceLeaf, item);

            if (CollectionUtil.NullOrEmpty(referenceLeaf.Leafs)) {
                return clonedItem;
            }

            return MergeLeafNodes(clonedItem);
        }

        private static MenuBaseDefinition MergeLeafNodes(MenuBaseDefinition clonedItem) {
            //TODO: finish merge applying positions to leafts, etc
            return clonedItem;
        }

        private static MenuBaseDefinition MergeOwnProperties(ReferenceMenuItemDefinition referenceLeaf, MenuBaseDefinition item) {
            var clonedItem = item.ShallowCopy();
            clonedItem.Title = referenceLeaf.Title ?? clonedItem.Title;
            clonedItem.Role = referenceLeaf.Role ?? clonedItem.Role;
            clonedItem.Icon = referenceLeaf.Icon ?? clonedItem.Icon;
            clonedItem.Tooltip = referenceLeaf.Tooltip ?? clonedItem.Tooltip;
            if (clonedItem is MenuContainerDefinition) {
                var container = clonedItem as MenuContainerDefinition;
                container.Action = referenceLeaf.Action ?? container.Action;
                container.Controller = referenceLeaf.Action ?? container.Controller;
            }
            return clonedItem;
        }
    }
}
