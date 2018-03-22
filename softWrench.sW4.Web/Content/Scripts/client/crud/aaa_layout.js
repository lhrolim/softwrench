﻿(function (angular) {
    "use strict";

    // solves conflict between jquery-ui and bootstrap
    const bootstrapButton = $.fn.button.noConflict(); // return $.fn.button to previously assigned value
    $.fn.bootstrapBtn = bootstrapButton;            // give $().bootstrapBtn the Bootstrap functionality
    
    angular.module("sw_prelogin", ["webcommons_services", "ngSanitize", "angular-clipboard", "ui.tinymce"]);

    const app = window.app = angular.module("sw_layout", [
        "sw_rootcommons",
        "sw_prelogin",
        "pasvaz.bindonce",
        "angularTreeview",
        "ngSanitize",
        "textAngular",
        "angularFileUpload",
        "angular-clipboard",
        "angular-click-outside",
        "pdf",
        "xeditable",
        "DeferredWithUpdate",
        "sw_lookup",
        "sw_typeahead",
        "sw_scan",
        "sw_crudadmin",
        "sw_components",
        "dynforms",
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



    function LayoutController($scope, $http, $log, $templateCache, $q, $rootScope, $timeout, fixHeaderService, redirectService, i18NService, menuService, contextService, spinService, schemaCacheService, logoutService, crudContextHolderService, schemaService) {

        $scope.$name = 'LayoutController';
        schemaCacheService.wipeSchemaCacheIfNeeded();



        $scope.isDesktop = function () {
            return isDesktop();
        };

        $scope.contextPath = function (path) {
            return url(path);
        };

        $scope.goToApplicationView = function (applicationName, schemaId, mode, title, parameters, $event) {
            menuService.setActiveLeaf($event.target,null);
            redirectService.goToApplicationView(applicationName, schemaId, mode, title, parameters);
        };

        $scope.doubleClickDispatched = function ($event) {
            $rootScope.$broadcast(JavascriptEventConstants.FormDoubleClicked, $event,true);
        }

        //#region listeners


   

        $scope.$on(JavascriptEventConstants.AjaxInit, function () {
            const savingMain = true === $rootScope.savingMain;
            spinService.start({ savingDetail: savingMain });

        });

        $scope.$on(JavascriptEventConstants.AjaxFinished, function () {
            $log.get("sw4layout.ajaxfinished", ["layout"]).debug("ajax finished");
            spinService.stop();
            $rootScope.savingMain = undefined;
//            fixHeaderService.callWindowResize();
        });

        $scope.$on(JavascriptEventConstants.DetailLoaded, () => {
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
            fixHeaderService.callWindowResize();
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



        var rectangleselectiondiv = document.getElementById('rectangleselectiondiv'), x1 = 0, y1 = 0, x2 = 0, y2 = 0;

        function reCalc() {
            const x3 = Math.min(x1, x2);
            const x4 = Math.max(x1, x2);
            const y3 = Math.min(y1, y2);
            const y4 = Math.max(y1, y2);
            rectangleselectiondiv.style.left = x3 + 'px';
            rectangleselectiondiv.style.top = y3 + 'px';
            rectangleselectiondiv.style.width = x4 - x3 + 'px';
            rectangleselectiondiv.style.height = y4 - y3 + 'px';
        }

        $scope.mousedown = function (e) {
            if (!schemaService.isPropertyTrue(crudContextHolderService.currentSchema(), "dynforms.editionallowed")) {
                //only on dynamic form edition we shall allow this for now
                return;
            }

            rectangleselectiondiv.hidden = 0;
            x1 = e.clientX;
            y1 = e.clientY;
            reCalc();
        }

        $scope.mouseup = function (e) {
            rectangleselectiondiv.hidden = 1;
            const points = {
                x1,
                x2,
                y1,
                y2
            }
            $rootScope.$broadcast("sw_rectangleselection_finished", points);
            x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        }

        $scope.mousemove = function (e) {
            x2 = e.clientX;
            y2 = e.clientY;
            reCalc();
        }


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

            $rootScope.deviceType = DeviceDetect.catagory.toLowerCase();
            $rootScope.browserType = BrowserDetect.browser.toLowerCase();

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
                $scope.isDynamicAdmin = menuModel.isDynamicAdmin || contextService.isLocal();
                $scope.isClientAdmin = menuModel.isClientAdmin;
                $scope.myprofileenabled = menuModel.myProfileEnabled;
                $('.hapag-body').addClass('hapag-body-loaded');
            });


        }

        $scope.i18N = function (key, defaultValue, paramArray) {
            return i18NService.get18nValue(key, defaultValue, paramArray);
        };

        initController();
    }

    app.controller("LayoutController", ["$scope", "$http", "$log", "$templateCache", "$q", "$rootScope", "$timeout", "fixHeaderService", "redirectService", "i18NService", "menuService", "contextService", "spinService", "schemaCacheService", "logoutService", "crudContextHolderService","schemaService", LayoutController]);

})(angular);