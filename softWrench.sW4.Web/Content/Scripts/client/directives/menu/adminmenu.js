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

            scope.loadApplication = function (applicationName, schemaId, mode, id) {
                var parameters = {
                    Id: id
                }
                redirectService.goToApplicationView(applicationName, schemaId, mode, null, parameters, null);
            };

            scope.myProfile = function () {
                var crudContext = {
                    detail_next: "0",
                    detail_previous: "-1"
                };
                contextService.insertIntoContext("crud_context", crudContext);
                var id = contextService.getUserData().maximoPersonId;
                this.loadApplication('Person', 'detail', 'input', id);
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
                sessionStorage.removeItem("swGlobalRedirectURL");
                contextService.clearContext();
                sessionStorage['ctx_loggedin'] = false;
            };

            //show or hide the menu when the expand button is clicked
            $('.menu-expand').click(function () {
                jQuery(this).toggleClass('menu-open');
            });
        }
    };
});