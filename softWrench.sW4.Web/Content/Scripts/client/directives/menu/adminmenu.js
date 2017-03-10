(function (app) {
    "use strict";

    app.directive('adminMenu', ["contextService", "menuService", "redirectService", "i18NService", "schemaCacheService", "adminMenuService", "sidePanelService",
    function (contextService, menuService, redirectService, i18NService, schemaCacheService, adminMenuService, sidePanelService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/menu/adminmenu.html'),
        scope: {
            isClientAdmin: '=',
            isSysAdmin: '=',
            isDynamicAdmin: '=',
            popupmode: '=',
            myprofileenabled: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {
            scope.doAction = function (title, controller, action, parameters, $event) {
                var target = $event ? $event.target : null;
                adminMenuService.doAction(title, controller, action, parameters, target).then(function() {
                });
            };

            scope.refreshMetadata = function() {
                this.doAction(null, 'EntityMetadata', 'Refresh', null, null);
                schemaCacheService.wipeSchemaCacheIfNeeded(true);
                window.location.reload();
            }

            scope.loadApplication = function (applicationName, schemaId, mode, id) {
                adminMenuService.loadApplication(applicationName, schemaId, mode, id);
            };

            scope.myProfile = function () {
                adminMenuService.myProfile();
            }

            scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            scope.userInfo = function() {
                var user = contextService.getUserData();
                return user.firstName + ' ' + user.lastName;
            }

            scope.contextPath = function (path) {
                return url(path);
            };

            scope.isLocal= function (path) {
                return contextService.isLocal();
            };

            scope.logout = function () {
                adminMenuService.logout();
            };

            scope.showClassicMenu = function () {
                return contextService.fetchFromContext('UIShowClassicAdminMenu', false, true);
            };

            // adds a padding right to not be behind side panels handles
            scope.sidePanelStyle = function () {
                var style = {};
                if (sidePanelService.getNumberOfVisiblePanels() > 0) {
                    style["padding-right"] = "40px";
                }
                return style;
            }

            scope.showLabel = function () {
                //use global property to hide/show labels
                return contextService.getFromContext("UIShowToolbarLabels", false, true);
            }
        }
    };
}]);

app.directive('menuExpander', function () {
    return {
        link: function (scope, element, attr) {
            scope.clickMenuExpander = function () {
                $(element).toggleClass('menu-open');
            };
        }
    };
});

})(app);