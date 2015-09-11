using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Menu {
    class MenuCatalogMerger {


        public static List<MenuBaseDefinition> MergeWithCatalog(MenuTemplateCatalog catalog, List<MenuBaseDefinition> leafs) {
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

        private static MenuBaseDefinition GetMergedDefinition(ReferenceMenuItemDefinition referenceLeaf, MenuTemplateCatalog catalog) {
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
