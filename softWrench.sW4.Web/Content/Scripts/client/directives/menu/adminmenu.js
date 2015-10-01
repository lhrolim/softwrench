app.directive('adminMenu', function (contextService, menuService, redirectService, i18NService, schemaCacheService, adminMenuService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/menu/adminmenu.html'),
        scope: {
            isClientAdmin: '=',
            isSysAdmin: '=',
            popupmode: '=',
            myprofileenabled: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {
            scope.doAction = function (title, controller, action, parameters, $event) {
                var target = $event ? $event.target : null;
                adminMenuService.doAction(title, controller, action, parameters, target);
            };

            scope.refreshMetadata = function() {
                this.doAction(null, 'EntityMetadata', 'Refresh', null, null);
                schemaCacheService.wipeSchemaCacheIfNeeded(true);
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
        }
    };
});

app.directive('menuExpander', function () {
    return {
        link: function (scope, element, attr) {
            scope.clickMenuExpander = function () {
                $(element).toggleClass('menu-open');
            };
        }
    };
});
