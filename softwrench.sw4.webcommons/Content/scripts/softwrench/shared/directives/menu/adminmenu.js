app.directive('adminMenu', function (contextService,menuService,redirectService,i18NService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/menu/adminmenu.html'),
        scope: {
            isClientAdmin: '=',
            isSysAdmin: '=',
            popupmode: '=',
            myprofileenabled:'='
        },
        link: function(scope,element,attr) {

            scope.doAction = function (title, controller, action, parameters, target) {
                menuService.setActiveLeaf(target);
                redirectService.redirectToAction(title, controller, action, parameters);
            };

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

            scope.logout = function () {
                sessionStorage.removeItem("swGlobalRedirectURL");
                contextService.clearContext();
                sessionStorage['ctx_loggedin'] = false;
            };
            
        }
    };
});