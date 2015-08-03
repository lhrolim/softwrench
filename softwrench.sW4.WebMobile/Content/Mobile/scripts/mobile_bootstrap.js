﻿var swdb;

//window.ionic.Platform.ready(function () {
//    //initing the softwrench application programatically instead of using ng-app for the sake of persistence.js ripple/device compatibility
//    angular.bootstrap(document, ['softwrench']);
//});

//#region 'deviceready' listener
document.addEventListener("deviceready", function() {
    // retrieve the DOM element that had the ng-app attribute
    angular.bootstrap(document.body, ["softwrench"]);
}, false);
//#endregion

//#region Global functions
/**
 * Function that returns the angular $scope attached to an element.
 * It helps debug the app when deployed in Ripple (batarang's $scope inspection doesn't work in iframe);
 * 
 * @param {} element DOM element 
 * @returns {} $scope 
 */
var $s = function (element) {
    var elementWrapper = angular.element(element);
    if (typeof (elementWrapper['scope']) !== "function") {
        return null;
    }
    var scope = elementWrapper.scope();
    if (!scope || !scope['$parent']) {
        return scope;
    }
    return scope.$parent;
};
//#endregion

//#region App Modules

var mobileServices = angular.module('sw_mobile_services', ['webcommons_services', 'maximo_applications', 'persistence.offline', 'audit.offline']);
var offlineMaximoApplications = angular.module('maximo_offlineapplications', ['persistence.offline', 'audit.offline']);
var softwrench = angular.module('softwrench', ['ionic', 'ion-autocomplete', 'ngCordova', 'sw_mobile_services', 'webcommons_services', 'maximo_applications', 'maximo_offlineapplications','sw_scan'])
//#endregion

//#region App.run
.run(["$ionicPlatform", "swdbDAO", "$log", "securityService", "contextService", "menuModelService", "metadataModelService", "routeService", "crudContextService", "synchronizationNotificationService", "offlinePersitenceBootstrap", "offlineEntities",
    function ($ionicPlatform, swdbDAO, $log, securityService, contextService, menuModelService, metadataModelService, routeService, crudContextService, synchronizationNotificationService, offlinePersitenceBootstrap, entities) {

    $ionicPlatform.ready(function () {
        // if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
        //      WebView.setWebContentsDebuggingEnabled(true);
        // }

        initContext();
        initDataBaseDebuggingHelpers();

        initCordovaPlugins();

        var authenticated = securityService.hasAuthenticatedUser();
        crudContextService.restoreState();
        routeService.loadInitialState(authenticated);
    });

    function initContext() {
        var log = $log.get("bootstrap#initContext");

        offlinePersitenceBootstrap.init();

        menuModelService.initAndCacheFromDB();

        metadataModelService.initAndCacheFromDB();

        swdbDAO.findAll("Settings").then(function (settings) {
            if (settings.length <= 0) {
                log.info('creating infos for the first time');
                var ob = entities.Settings;
                swdbDAO.save(new ob()).then(function (savedSetting) {
                    contextService.insertIntoContext("settings", savedSetting);
                });
            } else {
                log.info('loading settings');
                contextService.insertIntoContext("settings", settings[0],true);
                contextService.insertIntoContext("serverurl", settings[0].serverurl);
            }
        });
    }

    function initDataBaseDebuggingHelpers() {
        // DataBase debug mode: set swdbDAO service as global variable
        if (!!persistence.debug) {
            window.swdbDAO = swdbDAO;
        }
    }

    function initCordovaPlugins() {
        var log = $log.get("bootstrap#initCordovaPlugins");
        // Hide the accessory bar by default (remove this to show the accessory bar above the keyboard
        // for form inputs)
        log.info("init cordova plugins");
        if (window.cordova && window.cordova.plugins.Keyboard) {
            cordova.plugins.Keyboard.hideKeyboardAccessoryBar(true);
        }
        // if (window.StatusBar) {
        //      // org.apache.cordova.statusbar required
        //      StatusBar.styleDefault();
        // }


        /* LOCAL NOTIFICATION */
        synchronizationNotificationService.prepareNotificationFeature();
    };

}])
//#endregion

//#region App.config
.config(["$stateProvider", "$urlRouterProvider", "$logProvider", "$ionicConfigProvider", function ($stateProvider, $urlRouterProvider, $logProvider, $ionicConfigProvider) {

    // center page titles
    $ionicConfigProvider.navBar.alignTitle("center");

    $logProvider.debugEnabled(true);

    // Ionic uses AngularUI Router which uses the concept of states
    // Learn more here: https://github.com/angular-ui/ui-router
    // Set up the various states which the app can be in.
    // Each state's controller can be found in controllers.js
    $stateProvider
        .state('login', {
            url: '/login',
            templateUrl: 'Content/Mobile/templates/login.html',
            controller: 'LoginController',
            params: { message: null }
        })
        .state('settings', {
            url: '/settings',
            templateUrl: 'Content/Mobile/templates/settings_nomenu.html',
            controller: 'SettingsController'
        })

        // setup an abstract state for the tabs directive
        .state('main', {
            url: "/main",
            templateUrl: "Content/Mobile/templates/main.html",
            //abstract: true,
            controller: 'MainController'
        })
        .state('main.home', {
            url: '/home',
            views: {
                'main': {
                    templateUrl: 'Content/Mobile/templates/syncoperation_detail.html',
                    controller: 'SyncOperationDetailController'
                }
            }
        })
        .state('main.syncoperationhistory', {
            url: '/syncoperationhistory',
            views: {
                'main': {
                    templateUrl: 'Content/Mobile/templates/syncoperation_list.html',
                    controller: 'SyncOperationHistoryController'
                }
            }
        })
        .state('main.syncoperationdetail', {
            url: '/syncoperationdetail/{id}',
            views: {
                'main': {
                    templateUrl: 'Content/Mobile/templates/syncoperation_detail.html',
                    controller: 'SyncOperationDetailController'
                }
            }
        })
        .state('main.settings', {
            url: '/settings',
            views: {
                'main': {
                    templateUrl: 'Content/Mobile/templates/settings.html',
                    controller: 'SettingsController'
                }
            }
        })
        .state('main.crudlist', {
            url: "/crudlist",
            views: {
                'main': {
                    templateUrl: "Content/Mobile/templates/crudlist.html",
                    controller: 'CrudListController'
                }
            }
        })
        .state('main.cruddetail', {
            url: "/cruddetail",
            views: {
                'main': {
                    templateUrl: "Content/Mobile/templates/crud_detail.html",
                    controller: 'CrudDetailController'
                },

            }
        })
        .state('main.cruddetail.maininput', {
            url: "/crudinput",
            views: {
                'body': {
                    templateUrl: "Content/Mobile/templates/crud_input.html",
                    controller: 'CrudInputController'
                }
            }
        })
        .state('main.cruddetail.compositionlist', {
            url: "/crudcompositionlist",
            views: {
                'body': {
                    templateUrl: "Content/Mobile/templates/crud_composition_list.html",
                    controller: 'CrudCompositionListController'
                }
            }
        })
        .state('main.cruddetail.compositiondetail', {
            url: "/crudcompositionoutputdetail",
            views: {
                'body': {
                    templateUrl: "Content/Mobile/templates/crud_composition_detail.html",
                    controller: 'CrudCompositionDetailController'
                }
            }
        })
        // audit
        .state("main.audit", {
            'abstract': true,
            url: "/audit"
        })
        .state("main.audit.applicationselect", {
            url: "/application",
            views: {
                'main@main': { 
                    templateUrl: "Content/Mobile/templates/audit/audit.application.select.html",
                    controller: "AuditApplicationSelectController"
                }
            }
        })
        .state("main.audit.applicationselect.entrylist", {
            url: "/list/{application}",
            views: {
                'main@main': {
                    templateUrl: "Content/Mobile/templates/audit/audit.entry.list.html",
                    controller: "AuditEntryListController"
                }
            }
        })
        .state("main.audit.applicationselect.entrylist.entrydetail", {
            url: "/entry/{id}",
            views: {
                'main@main': {
                    templateUrl: "Content/Mobile/templates/audit/audit.entry.detail.html",
                    controller: "AuditEntryDetailController"
                }
            }
        });



    // if none of the above states are matched, use this as the fallback
    $urlRouterProvider.otherwise('/main/home');

}]);
//#endregion