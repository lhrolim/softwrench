(function (angular) {
    "use strict";

    var app = window.app = angular.module('sw_layout',
        ['pasvaz.bindonce',
         'angularTreeview',
         'ngSanitize',
         'textAngular',
         'angularFileUpload',
         'angular-clipboard',
         "xeditable",
         'sw_lookup',
         'sw_typeahead',
         "sw_scan",
         "sw_crudadmin",
         'webcommons_services',
         'maximo_applications',
         'selectize',
         'ngAnimate',
         'omr.angularFileDnD']);

    angular.module('sw_prelogin', []);

    //angular 1.3 migration reference
    //app.config(['$controllerProvider', function ($controllerProvider) {
    //    $controllerProvider.allowGlobals();
    //}]);

    //app.config(function(uiSelectConfig) {
    //    uiSelectConfig.theme = "bootstrap";
    //});

    app.run(["editableOptions", function (editableOptions) {
        editableOptions.theme = 'bs3'; // bootstrap3 theme. Can be also 'bs2', 'default'
    }]);


    //#region extra directives
    app.directive("dynamicName", function ($compile) {
        "ngInject";
        /// <summary>
        /// workaround for having dynamic named forms to work with angular 1.2
        /// took from http://jsfiddle.net/YAZmz/2/
        /// </summary>
        /// <param name="$compile"></param>
        /// <returns type=""></returns>
        return {
            restrict: "A",
            terminal: true,
            priority: 1000,
            link: function (scope, element, attrs) {
                element.attr('name', scope.$eval(attrs.dynamicName));
                element.removeAttr("dynamic-name");
                $compile(element)(scope);
            }
        };
    });


    app.filter('linebreak', function () {
        return function (value) {
            if (value != null) {
                value = value.toString();
                return value.replace(/\n/g, '<br/>');
            }
            return value;
        };
    });

    app.directive('swcontenteditable', function () {
        return {
            restrict: 'A',
            require: '?ngModel',
            link: function (scope, element, attr, ngModel) {
                var read;
                if (!ngModel) {
                    return;
                }
                ngModel.$render = function () {
                    return element.html(ngModel.$viewValue);
                };
                element.bind('blur', function () {
                    if (ngModel.$viewValue !== $.trim(element.html())) {
                        return scope.$apply(read);
                    }
                });
                return read = function () {
                    return ngModel.$setViewValue($.trim(element.html()));
                };
            }
        };
    });

    app.directive('onFinishRender', function ($timeout) {
        "ngInject";

        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$last === true) {
                    $timeout(function () {
                        scope.$emit('ngRepeatFinished');
                    });
                }
            }
        };
    });

    app.directive('ngEnter', function () {
        return function (scope, element, attrs) {
            element.bind("keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });

                    event.preventDefault();
                }
            });
        };
    });

    app.directive("ngEnabled", function () {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                scope.$watch(attrs.ngEnabled, function (val) {
                    if (val)
                        element.removeAttr("disabled");
                    else
                        element.attr("disabled", "disabled");
                });
            }
        };
    });

    app.directive('numberToString', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                // Some non-string values coming from the rootdatamap are breaking the string binding, so we need to format them to string
                // https://controltechnologysolutions.atlassian.net/browse/SWWEB-2042 : first issue
                ngModel.$formatters.push(function (value) {
                    if (value == null) {
                        return null;
                    }
                    return '' + value;
                });
            }
        };
    });

    //#endregion



    function LayoutController($scope, $http, $log, $templateCache, $q, $rootScope, $timeout, fixHeaderService, redirectService, i18NService, menuService, contextService, spinService, schemaCacheService, logoutService, crudContextHolderService) {

        $scope.$name = 'LayoutController';
        var log = $log.getInstance('sw4.LayoutController');

        schemaCacheService.wipeSchemaCacheIfNeeded();



        $scope.isDesktop = function () {
            return isDesktop();
        };

        $scope.contextPath = function (path) {
            return url(path);
        };

        $scope.goToApplicationView = function (applicationName, schemaId, mode, title, parameters, $event) {
            menuService.setActiveLeaf($event.target);
            redirectService.goToApplicationView(applicationName, schemaId, mode, title, parameters);
        };


        //#region listeners


   

        $rootScope.$on('sw_ajaxinit', function (ajaxinitevent) {
            var savingMain = true === $rootScope.savingMain;
            spinService.start({ savingDetail: savingMain });

        });

        $rootScope.$on('sw_ajaxend', function (data) {
            spinService.stop();
            $rootScope.savingMain = undefined;
            fixHeaderService.callWindowResize();
        });

        $rootScope.$on('sw_ajaxerror', function (data) {
            spinService.stop();
            $rootScope.savingMain = undefined;
            fixHeaderService.callWindowResize();
        });

        $scope.$on('ngLoadFinished', function (ngLoadFinishedEvent) {
            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
            menuService.adjustHeight();
        });

        $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {
            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });

            var sidebarWidth = $('.col-side-bar').width();
            if (sidebarWidth != null) {
                $('.col-main-content').css('margin-left', sidebarWidth);
            }
        });


        $scope.$on('sw_titlechanged', function (titlechangedevent, title) {
            var record = i18NService.getI18nRecordLabel(crudContextHolderService.currentSchema(), crudContextHolderService.rootDataMap());
            if (record) {
                title = record + ' | ' + title;
            }

            $scope.title = title;

            if (title) {
                window.document.title = title + ' | softWrench';
            } else {
                window.document.title = 'softWrench';
            }
        });


        $rootScope.$on('sw_redirectactionsuccess', function (event, result) {
            var log = $log.getInstance('layoutcontroller#onsw_redirectactionsuccess');
            log.debug("received event");
            $scope.AjaxResult(result);
        });

        $rootScope.$on('sw_redirectapplicationsuccess', function (event, result, mode, applicationName) {
            var log = $log.getInstance('layoutcontroller#onsw_redirectapplicationsuccess');
            //todo: are these 2 parameters really necessary?
            $scope.applicationname = applicationName;
            $scope.requestmode = mode;
            if ($rootScope.popupmode != undefined) {
                $scope.popupmode = $rootScope.popupmode;
                $(hddn_popupmode)[0].value = $rootScope.popupmode;
            } else {
                //keep the current popupmode
                $rootScope.popupmode = $scope.popupmode;
            }
            log.debug("received event");
            $scope.AjaxResult(result);
        });

        //#endregion


        $scope.AjaxResult = function (result) {
            var log = $log.getInstance('layoutcontroller#AjaxResult', ["redirect"]);
            var newUrl = url(result.redirectURL);
            if ($scope.includeURL !== newUrl) {
                log.debug("redirection detected new:{0} old:{1}".format(newUrl, $scope.includeURL));
                $scope.includeURL = newUrl;
            }

            if (result.title != null) {
                log.debug("dispatching title changed event. Title: {0}".format(result.title));
                $scope.$emit('sw_titlechanged', result.title);
            }
            $scope.resultData = result.resultObject;
            $scope.resultObject = result;

            // scroll window to top, in a timeout, in order to give time to the page/grid render
            $timeout(
                function () {
                    window.scrollTo(0, 0);
                }, 100, false);
        };



        $scope.resourceUrl = function (path) {
            return contextService.getResourceUrl(path);
        }

        function initController() {
            var configsJSON = $(hddn_configs)[0].value;
            var userJSON = $(hiddn_user)[0].value;
            if (nullOrEmpty(configsJSON) || nullOrEmpty(userJSON)) {
                //this means user tried to hit back button after logout
                logoutService.logout();
                return;
            }
            var config = JSON.parse(configsJSON);
            var user = JSON.parse(userJSON);
            contextService.loadUserContext(user);
            contextService.loadConfigs(config);

            contextService.insertIntoContext("isLocal", config.isLocal);
            $rootScope.defaultEmail = config.defaultEmail;
            $rootScope.clientName = config.clientName;
            $rootScope.environment = config.environment;
            $rootScope.i18NRequired = config.i18NRequired;
            $rootScope.deviceType = DeviceDetect.catagory.toLowerCase();
            contextService.insertIntoContext("activityStreamFlag", config.activityStreamFlag, true);
            contextService.insertIntoContext("UIShowClassicAdminMenu", config.uiShowClassicAdminMenu, true);

            $scope.mainlogo = config.logo;
            $scope.myprofileenabled = config.myProfileEnabled;

            $scope.$on('sw_goToApplicationView', function (event, data) {
                if (data != null) {
                    $scope.goToApplicationView(data.applicationName, data.schemaId, data.mode, data.title, data.parameters);
                }
            });

            var popupMode = GetPopUpMode();
            $scope.popupmode = popupMode;
            if (popupMode !== "none") {
                return;
            }

            $scope.$on("sw_loadmenu", function (event, menuModel) {
                $scope.$on('sw_indexPageLoaded', function (event, url) {
                    if (url != null) {
                        menuService.setActiveLeafByUrl(menuModel.menu, url);
                    }
                });
                contextService.insertIntoContext("commandbars", menuModel.commandBars);
                $rootScope.menu = menuModel.menu;
                $scope.menu = menuModel.menu;
                $scope.isSysAdmin = menuModel.isSysAdmin;
                $scope.isClientAdmin = menuModel.isClientAdmin;
                $('.hapag-body').addClass('hapag-body-loaded');
            });


        }

        $scope.i18N = function (key, defaultValue, paramArray) {
            return i18NService.get18nValue(key, defaultValue, paramArray);
        };

        initController();
    }

    app.controller("LayoutController", ["$scope", "$http", "$log", "$templateCache", "$q", "$rootScope", "$timeout", "fixHeaderService", "redirectService", "i18NService", "menuService", "contextService", "spinService", "schemaCacheService", "logoutService", "crudContextHolderService", LayoutController]);

})(angular);