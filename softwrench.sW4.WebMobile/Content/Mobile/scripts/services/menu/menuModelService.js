mobileServices.factory('menuModelService', function ($q, swdbDAO, $log) {

    var menuModel = {
        dbData: {},
        listItems: []
    };


    return {

        getMenuItens: function () {
            return menuModel.listItems;
        },

        updateMenu: function (serverMenu) {
            var defer = $q.defer();

            swdbDAO.instantiate("Menu", menuModel.dbData).success(function (menu) {
                menu.data = serverMenu;
                swdbDAO.save(menu).success(function (item) {
                    menuModel.dbData.data = serverMenu;
                    menuModel.listItems = serverMenu.explodedLeafs;
                    defer.resolve();
                });

            });

            return defer.promise;
        },

        initAndCacheFromDB: function () {
            var log = $log.getInstance("menuModelService#initAndCacheFromDB");
            var defer = $q.defer();
            swdbDAO.findUnique("Menu").success(function (menu) {
                if (!menu) {
                    menu = new entities.Menu();
                    swdbDAO.save(menu);
                    log.info('creating first menu');
                } else if (menu.data) {
                    menuModel.listItems = menu.data.explodedLeafs;
                }
                menuModel.dbData = menu;
                defer.resolve();
            });
            return defer.promise;
        },

    }
});