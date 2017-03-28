(function (mobileServices, _) {
    "use strict";

    function menuModelService(swdbDAO, $log, $injector, offlineEntities, offlineSchemaService, queryListBuilderService, dispatcherService, metadataModelService) {

        const initialMenuModel = {
            dbData: {},
            listItems: [],
            menuWc: {},
            appCount: {}
        };

        var menuModel = angular.copy(initialMenuModel);

        const reservedMenuContainers = {
            admin: "admin-section",
            user: "user-section"
        }

        function isReservedContainer(leaf) {
            return leaf.type === "MenuContainerDefinition" && _.contains(Object.values(reservedMenuContainers), leaf.id);
        }

        function isContainerMatchingId(leaf, id) {
            return leaf.type === "MenuContainerDefinition" && leaf.id === id;
        }

        function getMenuItems() {
            return menuModel.listItems;
        }

        function getApplicationMenuItems() {
            return getMenuItems().filter(leaf => !isReservedContainer(leaf));
        }

        function getReservedMenuContainers() {
            return getMenuItems().filter(isReservedContainer);
        }

        function getMenuContainerItems(id) {
            const container = getMenuItems().find(leaf => isContainerMatchingId(leaf, id));
            return container ? container.leafs : [];
        };

        function getAdminMenuItems() {
            return getMenuContainerItems(reservedMenuContainers.admin);
        }

        function getUserMenuItems() {
            return getMenuContainerItems(reservedMenuContainers.user);
        }

        function getAppCount(menuId) {
            return menuModel.appCount[menuId] || 0;
        }

        function buildListQuery(appName, menuId, extraWhereClause) {
            const menuWc = menuId && menuModel.menuWc[menuId] ? menuModel.menuWc[menuId] : "1=1";

            extraWhereClause = extraWhereClause || "1=1";

            //appending root prefix, since a left join could be present leading to ambiguity amongst columns
            return "`root`.application = '{0}' and ({1}) and ({2}) ".format(appName, menuWc, extraWhereClause);
        }

        function buildJoinObj(menu) {
            const application = metadataModelService.getApplicationByName(menu.application);
            if (!application) {
                return {};
            }
            const listSchema = offlineSchemaService.locateSchema(application, menu.schema);
            if (!listSchema) {
                return {};
            }
            return queryListBuilderService.buildJoinParameters(listSchema);
        }

        function updateAppCount(menu) {
            const joinObj = buildJoinObj(menu);
            if (menu.parameters && menu.parameters.offlinemenuwc) {
                menuModel.menuWc[menu.id] = dispatcherService.invokeServiceByString(menu.parameters.offlinemenuwc);
            }

            const query = buildListQuery(menu.application, menu.id);

            swdbDAO.countByQuery("DataEntry", query, joinObj).then((count) => {
                menuModel.appCount[menu.id] = count;
            });
        }

        function updateAppsCount() {
            const leafs = getApplicationMenuItems();
            angular.forEach(leafs, (leaf) => {
                if (leaf.type === "ApplicationMenuItemDefinition") {
                    updateAppCount(leaf);
                    return;
                }
                if (leaf.type !== "MenuContainerDefinition") {
                    return;
                }
                angular.forEach(leaf.explodedLeafs, (subLeaf) => {
                    if (subLeaf.type === "ApplicationMenuItemDefinition") {
                        updateAppCount(subLeaf);
                    }
                });
            });
        }

        function initAndCacheFromDB() {
            const log = $log.getInstance("menuModelService#initAndCacheFromDB", ["init", "metadata", "botstrap"]);
            return swdbDAO.findSingleByQuery("Menu","data is not null").then(menu => {
                if (!!menu) {
                    log.info("restoring menu");
                    menuModel.dbData = menu;
                }
                if (!menu) {
                    menu = new offlineEntities.Menu();
                    log.info("creating first menu");
                    return swdbDAO.save(menu);
                } else if (menu.data) {
                    log.info("restoring menu data");
                    menuModel.listItems = menu.data.leafs;
                }
                return menu;
            });
        }

        function updateMenu(serverMenu) {
            return !serverMenu || _.isEmpty(serverMenu)
                ? initAndCacheFromDB()
                : swdbDAO.instantiate("Menu", menuModel.dbData).then(menu => {
                    menu.data = serverMenu;
                    return swdbDAO.save(menu).then(item => {
                        menuModel.dbData.data = serverMenu;
                        menuModel.listItems = serverMenu.leafs;
                        return item;
                    });
                });
        }

        function reset() {
            menuModel = angular.copy(initialMenuModel);
        }

        const service = {
            getMenuItems,
            getApplicationMenuItems,
            getReservedMenuContainers,
            getMenuContainerItems,
            getAdminMenuItems,
            getUserMenuItems,
            buildListQuery,
            getAppCount,
            updateAppsCount,
            updateMenu,
            initAndCacheFromDB,
            reset
        };
        return service;
    };

    mobileServices.factory("menuModelService", ["swdbDAO", "$log", "$injector", "offlineEntities", "offlineSchemaService", "queryListBuilderService", "dispatcherService", "metadataModelService", menuModelService]);

})(mobileServices, _);