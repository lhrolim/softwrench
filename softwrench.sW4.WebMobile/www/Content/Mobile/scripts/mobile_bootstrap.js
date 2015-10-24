//#region 'deviceready' listener
document.addEventListener("deviceready", function () {
    // retrieve the DOM element that had the ng-app attribute
    // bootstrap angular app "softwrench" programatically
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

var mobileServices = angular.module('sw_mobile_services', ['webcommons_services', 'maximo_applications', 'persistence.offline', 'audit.offline', "rollingLog"]);
var offlineMaximoApplications = angular.module('maximo_offlineapplications', ['persistence.offline', 'audit.offline']);
var softwrench = angular.module('softwrench', ['ionic', 'ion-autocomplete', 'ngCordova', 'sw_mobile_services', 'webcommons_services', 'maximo_applications', 'maximo_offlineapplications', 'sw_scan'])
//#endregion

//#region App.run
.run(["$ionicPlatform", "swdbDAO", "$log", "securityService",
    "localStorageService", "menuModelService", "metadataModelService", "routeService",
    "crudContextService", "synchronizationNotificationService",
    "offlinePersitenceBootstrap", "offlineEntities", "configurationService", "$rootScope", "$q",
    "$cordovaSplashscreen", "$timeout",
    function ($ionicPlatform, swdbDAO, $log, securityService, localStorageService, menuModelService, metadataModelService, routeService, crudContextService, synchronizationNotificationService, offlinePersitenceBootstrap,
        entities, configService, $rootScope, $q, $cordovaSplashscreen, $timeout) {

        function initContext() {
            offlinePersitenceBootstrap.init();
            var menuPromise = menuModelService.initAndCacheFromDB();
            var metadataPromise = metadataModelService.initAndCacheFromDB();
            //server side + client side configs
            var serverConfigPromise = configService.loadConfigs();
            var clientConfigPromise = configService.loadClientConfigs();
            return $q.all([menuPromise, metadataPromise, serverConfigPromise, clientConfigPromise]);
        }

        function initDataBaseDebuggingHelpers() {
            // DataBase debug mode: set swdbDAO service as global variable
            if (!!persistence.debug) {
                window.swdbDAO = swdbDAO;
            }
        }

        function disableRipplePopup() {
            var dialogBody = parent.document.getElementById("exec-dialog");
            var overlay = parent.document.querySelector(".ui-widget-overlay");
            var ngDialog = angular.element(dialogBody.parentElement);
            var ngOverlay = angular.element(overlay);
            var hideRules = { "height": "0px", "width": "0px", "display": "none" };
            ngDialog.css(hideRules); // hide annoying popup
            ngOverlay.css(hideRules); // hide annoying popup's backdrop
        }

        function initCordovaPlugins() {
            var log = $log.get("bootstrap#initCordovaPlugins");
            log.info("init cordova plugins");
            
            // first of all let's schedule the disable of ripple's annoying popup 
            // that tells us about unregistered plugins
            if(isRippleEmulator()) $timeout(disableRipplePopup);

            // Hide the accessory bar by default (remove this to show the accessory bar above the keyboard
            // for form inputs)
            if (window.cordova && window.cordova.plugins.Keyboard) {
                cordova.plugins.Keyboard.hideKeyboardAccessoryBar(true);
            }
            // necessary to set fullscreen on Android in order for android:softinput=adjustPan to work
            if (ionic.Platform.isAndroid()) {
                ionic.Platform.isFullScreen = true;
            }
            // local notification 
            synchronizationNotificationService.prepareNotificationFeature();
        }

        function attachEventListeners() {
            // don't allow going to 'login' or 'settings' if the user is still logged
            $rootScope.$on("$stateChangeStart", function (event, toState, toParams, fromState, fromParams) {
                // not going to 'login' nor 'settings' -> do nothing
                if (toState.name.indexOf("login") < 0 && toState.name !== "settings") {
                    return;
                }
                // going to 'login' or 'settings' and no user authenticated -> allow transition
                if (!securityService.hasAuthenticatedUser()) {
                    return;
                }
                // going to login and user is authenticated -> prevent transition
                event.preventDefault();
            });
            // go to settings prior to going to login if no settings is set
            $rootScope.$on("$stateChangeSuccess", function (event, toState, toParams, fromState, fromParams) {
                // not going to 'login' or coming from 'settings' -> do nothing
                if (toState.name.indexOf("login") < 0 || fromState.name.indexOf("settings") >= 0) {
                    return;
                }
                // has serverurl -> do nothing
                var serverurl = localStorageService.get("settings:serverurl");
                if (!!serverurl) {
                    return;
                }
                // prevent state change
                event.preventDefault();
                // go to settings instead
                routeService.go("settings");
            });
        }

        function loadInitialState() {
            var authenticated = securityService.hasAuthenticatedUser();
            crudContextService.restoreState();
            return routeService.loadInitialState(authenticated);
        }

        $ionicPlatform.ready(function () {
            // loading eventual db stored values into context
            initContext().then(function () {
                attachEventListeners();
                initCordovaPlugins();
                initDataBaseDebuggingHelpers();
                return loadInitialState();
            }).then(function() {
                $timeout(function() {
                    $cordovaSplashscreen.hide();
                }, 1000); // 1 second delay to prevent blank screen right after hiding the splash screen (empirically determined)
            });
        });

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
            cache: false,
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

}]);
//#endregion
