(function (mobileServices) {
    "use strict";

    mobileServices.factory("menuModelService", ["swdbDAO", "$log", "offlineEntities", function (dao, $log, offlineEntities) {

        var initialMenuModel = {
            dbData: {},
            listItems: []
        };

        var menuModel = angular.copy(initialMenuModel);

        var entities = offlineEntities;

        return {

            getMenuItens: function () {
                return menuModel.listItems;
            },

            updateMenu: function (serverMenu) {
                return !serverMenu || _.isEmpty(serverMenu)
                    ? this.initAndCacheFromDB()
                    : dao.instantiate("Menu", menuModel.dbData).then(menu => {
                        menu.data = serverMenu;
                        return dao.save(menu).then(item => {
                            menuModel.dbData.data = serverMenu;
                            menuModel.listItems = serverMenu.leafs;
                            return item;
                        });
                    });
            },

            initAndCacheFromDB: function () {
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
            },

            reset: function () {
                menuModel = angular.copy(initialMenuModel);
            }

        }
    }]);

})(mobileServices);