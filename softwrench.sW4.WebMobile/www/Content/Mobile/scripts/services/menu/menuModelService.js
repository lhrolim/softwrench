(function (mobileServices, _) {
    "use strict";

    function menuModelService(dao, $log, entities) {

        const initialMenuModel = {
            dbData: {},
            listItems: [],
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

        function getAppCount(appName) {
            return menuModel.appCount[appName] || 0;
        }

        function updateAppCount(appName) {
            dao.executeStatement("select count(*) from DataEntry where application = :p0", [appName]).then((result) => {
                if (result[0] && result[0].hasOwnProperty("count(*)")) {
                    menuModel.appCount[appName] = result[0]["count(*)"];
                }
            });
        }

        function updateAppsCount() {
            const leafs = getApplicationMenuItems();
            angular.forEach(leafs, (leaf) => {
                if (leaf.type === "ApplicationMenuItemDefinition") {
                    updateAppCount(leaf.application);
                    return;
                }
                if (leaf.type !== "MenuContainerDefinition") {
                    return;
                }
                angular.forEach(leaf.explodedLeafs, (subLeaf) => {
                    if (subLeaf.type === "ApplicationMenuItemDefinition") {
                        updateAppCount(subLeaf.application);
                    }
                });
            });
        }

        function initAndCacheFromDB() {
            const log = $log.getInstance("menuModelService#initAndCacheFromDB");
            return dao.findUnique("Menu").then(menu => {
                if (!!menu) {
                    menuModel.dbData = menu;
                }
                if (!menu) {
                    menu = new entities.Menu();
                    log.info("creating first menu");
                    return dao.save(menu);
                } else if (menu.data) {
                    menuModel.listItems = menu.data.leafs;
                }
                updateAppsCount();
                return menu;
            });
        }

        function updateMenu(serverMenu) {
            return !serverMenu || _.isEmpty(serverMenu)
                ? initAndCacheFromDB()
                : dao.instantiate("Menu", menuModel.dbData).then(menu => {
                    menu.data = serverMenu;
                    return dao.save(menu).then(item => {
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
            getAppCount,
            updateAppsCount,
            updateMenu,
            initAndCacheFromDB,
            reset
        };
        return service;
    };

    mobileServices.factory("menuModelService", ["swdbDAO", "$log", "offlineEntities", menuModelService]);

})(mobileServices, _);