(function (mobileServices, _) {
    "use strict";

    function menuModelService(dao, $log, entities) {

        const initialMenuModel = {
            dbData: {},
            listItems: []
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
            updateMenu,
            initAndCacheFromDB,
            reset
        };
        return service;
    };

    mobileServices.factory("menuModelService", ["swdbDAO", "$log", "offlineEntities", menuModelService]);

})(mobileServices, _);