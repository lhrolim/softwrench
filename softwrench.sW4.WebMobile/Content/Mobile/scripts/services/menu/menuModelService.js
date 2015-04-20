mobileServices.factory('menuModelService', function ($q,swdbDAO) {

    var menuModel = {
        dbData: {},
        listItems: [
        { title: "WorkOrder" },
        { title: "Service Request" }]
    };


    return {

        getMenuItens:function() {
            return menuModel.listItems;
        },

        updateMenu: function (serverMenu) {
            var defer = $q.defer();
            
            swdbDAO.instantiate("Menu", menuModel.dbData).success(function(menu) {
                menu.data = serverMenu;
                swdbDAO.save(menu).success(function(item) {
                    menuModel.dbData.data = serverMenu;
                    menuModel.listItems = serverMenu.explodedLeafs;
                    defer.resolve();
                });
              
            });

            return defer.promise;
        },

        initAndCacheFromDB: function () {
            var defer = $q.defer();
            swdbDAO.findUnique("Menu").success(function (menu) {
                if (!menu) {
                    menu = new entities.Menu();
                    swdbDAO.save(menu);
                }
                menuModel.dbData = menu;
                menuModel.listItems = menu.data.explodedLeafs;
                defer.resolve();
            });
            return defer.promise;
        },

    }
});