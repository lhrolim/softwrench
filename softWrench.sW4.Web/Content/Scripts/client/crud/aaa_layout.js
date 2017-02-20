(function (angular) {
    "use strict";
    
    angular.module("sw_prelogin", ["webcommons_services", "ngSanitize", "angular-clipboard", "ui.tinymce"]);

    const app = window.app = angular.module("sw_layout", [
        "sw_prelogin", 
        "pasvaz.bindonce",
        "angularTreeview",
        "ngSanitize",
        "textAngular",
        "angularFileUpload",
        "angular-clipboard",
        "xeditable",
        "sw_lookup",
        "sw_typeahead",
        "sw_scan",
        "sw_crudadmin",
        "sw_components",
        "webcommons_services",
        "maximo_applications",
        "selectize",
        "ngAnimate",
        "omr.angularFileDnD",
        "dndLists",
        "ui.tinymce",
        "ui.ace",
        "ui.sortable",
        'ui.grid',
        'ui.grid.pagination',
        'ui.grid.edit',
        'ui.grid.cellNav'
    ]);

    app.run(["editableOptions", function (editableOptions) {
        editableOptions.theme = "bs3"; // bootstrap3 theme. Can be also 'bs2', 'default'
    }]);

    app.config(function($locationProvider, $httpProvider) {
        $locationProvider.html5Mode(true);
        $httpProvider.useApplyAsync(true);
    });

    //#region extra directives


    app.filter('linebreak', function () {
        return function (value) {
            if (value != null) {
                value = value.toString();
                return value.replace(/\n/g, '<br/>');
            }
            return value;
        };
    });

    //#endregion



    function LayoutController($scope, $http, $log, $templateCache, $q, $rootScope, $timeout, fixHeaderService, redirectService, i18NService, menuService, contextService, spinService, schemaCacheService, logoutService, crudContextHolderService) {

        $scope.$name = 'LayoutController';
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


   

        $scope.$on(JavascriptEventConstants.AjaxInit, function () {
            const savingMain = true === $rootScope.savingMain;
            spinService.start({ savingDetail: savingMain });

        });

        $scope.$on(JavascriptEventConstants.AjaxFinished, function () {
            spinService.stop();
            $rootScope.savingMain = undefined;
            fixHeaderService.callWindowResize();
        });

        $scope.$on(JavascriptEventConstants.ErrorAjax, function () {
            spinService.stop();
            $rootScope.savingMain = undefined;
            fixHeaderService.callWindowResize();
        });

        $scope.$on('ngLoadFinished', function () {
            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
            menuService.adjustHeight();
        });

        $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {
            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
            const sidebarWidth = $('.col-side-bar').width();
            if (sidebarWidth != null) {
                $('.col-main-content').css('margin-left', sidebarWidth);
            }
        });


        $scope.$on(JavascriptEventConstants.TitleChanged, function (titlechangedevent, title) {
            const record = i18NService.getI18nRecordLabel(crudContextHolderService.currentSchema(), crudContextHolderService.rootDataMap());
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


        $scope.$on(JavascriptEventConstants.ActionAfterRedirection, function (event, result) {
            const log = $log.getInstance('layoutcontroller#onsw_redirectactionsuccess', ["navigation", "route"]);
            log.debug("received event");
            $scope.AjaxResult(result);
        });

        $scope.$on(JavascriptEventConstants.REDIRECT_AFTER, function (event, result, mode, applicationName) {
            const log = $log.getInstance('layoutcontroller#onsw_redirectapplicationsuccess',["navigation","route"]);
            //todo: are these 2 parameters really necessary?
            $scope.applicationname = applicationName;
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
            const log = $log.getInstance('layoutcontroller#AjaxResult', ["redirect","navigation","route"]);
            const newUrl = url(result.redirectURL);
            if ($scope.includeURL !== newUrl) {
                log.debug("redirection detected new:{0} old:{1}".format(newUrl, $scope.includeURL));
                $scope.includeURL = newUrl;
            }

            if (result.title != null) {
                log.debug("dispatching title changed event. Title: {0}".format(result.title));
                $scope.$emit(JavascriptEventConstants.TitleChanged, result.title);
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

            const log = $log.get("LayoutController#init", ["init", "navigation", "route"]);
            log.debug("init Layout controller");

            const configsJSON = $(hddn_configs)[0].value;
            const userJSON = $(hiddn_user)[0].value;
            if (nullOrEmpty(configsJSON) || nullOrEmpty(userJSON) || contextService.get("sw:changepasword")) {
                contextService.deleteFromContext("sw:changepasword");
                //this means user tried to hit back button after logout
                logoutService.logout();
                return;
            }
            const config = JSON.parse(configsJSON);
            const user = JSON.parse(userJSON);
            contextService.loadUserContext(user);
            contextService.loadConfigs(config);

            
            $rootScope.defaultEmail = config.defaultEmail;
            $rootScope.clientName = config.clientName;
            $rootScope.environment = config.environment;
            $rootScope.i18NRequired = config.i18NRequired;
            $rootScope.deviceType = DeviceDetect.catagory.toLowerCase();
            $rootScope.browserType = BrowserDetect.browser.toLowerCase();



            $scope.mainlogo = config.logo;
            $scope.myprofileenabled = config.myProfileEnabled;

            const popupMode = GetPopUpMode();
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
                $scope.isDynamicAdmin = menuModel.isDynamicAdmin;
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