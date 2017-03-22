//#region 'deviceready' listener
document.addEventListener("deviceready", function () {
    // retrieve the DOM element that had the ng-app attribute
    // bootstrap angular app "softwrench" programatically
    angular.bootstrap(document.body, ["softwrench"]);
}, false);
//#endregion




//#region App Modules
var mobileServices = angular.module('sw_mobile_services', ['sw_rootcommons', 'webcommons_services', 'maximo_applications', 'persistence.offline', 'audit.offline', "rollingLog"]);
var offlineMaximoApplications = angular.module('maximo_offlineapplications', ['persistence.offline', 'audit.offline']);
var softwrench = angular.module('softwrench', ['ionic', 'ion-autocomplete', 'ngCordova', 'sw_mobile_services', 'webcommons_services', 'sw_rootcommons', 'maximo_applications', 'maximo_offlineapplications', 'sw_scan', 'ng-mfb', "ui.tinymce", "ngTouch"])
//#endregion

//#region App.run
.run(["$ionicPlatform", "swdbDAO", "$log", "securityService",
    "localStorageService", "menuModelService", "metadataModelService", "routeService",
    "crudContextService", "synchronizationNotificationService",
    "offlinePersitenceBootstrap", "offlineEntities", "configurationService", "$rootScope", "$q",
    "$cordovaSplashscreen", "$timeout", "offlineCommandService", "$ionicScrollDelegate", "trackingService", "initialRouterService",
    function ($ionicPlatform, swdbDAO, $log, securityService, localStorageService, menuModelService, metadataModelService, routeService, crudContextService, synchronizationNotificationService, offlinePersitenceBootstrap,
        entities, configService, $rootScope, $q, $cordovaSplashscreen, $timeout, offlineCommandService, $ionicScrollDelegate, trackingService, initialRouterService) {

        var initialHref = null;

        function initContext() {

            const localdata = window.localdevdata;
            if (localdata && !!localdata.debuglogs) {
                const debugarr = localdata.debuglogs;
                debugarr.forEach(log => {
                    swlog.debug(log);
                });
            }


            initialHref= window.location.href;
            trackingService.enable();
            return offlinePersitenceBootstrap.init().then(() => {
                const menuPromise = menuModelService.initAndCacheFromDB();
                const metadataPromise = metadataModelService.initAndCacheFromDB();
                const commandBarsPromise = offlineCommandService.initAndCacheFromDataBase();
                //server side + client side configs
                const serverConfigPromise = configService.loadConfigs();
                const clientConfigPromise = configService.loadClientConfigs();
                const restoreAuthPromise = securityService.restoreAuthCookie();

                return $q.all([menuPromise, metadataPromise, serverConfigPromise, commandBarsPromise, clientConfigPromise, restoreAuthPromise]);
            });
        }

        // keep startup url (in case your app is an SPA with html5 url routing)
        

        window.restartApplication= function () {
            // Show splash screen (useful if your app takes time to load) 
//            navigator.splashscreen.show();
            // Reload original app url (ie your index.html file)
            window.location = initialHref;
        }

        function disableRipplePopup() {
            const dialogBody = parent.document.getElementById("exec-dialog");
            const overlay = parent.document.querySelector(".ui-widget-overlay");
            const ngDialog = angular.element(dialogBody.parentElement);
            const ngOverlay = angular.element(overlay);
            const hideRules = { "height": "0px", "width": "0px", "display": "none" };
            ngDialog.css(hideRules); // hide annoying popup
            ngOverlay.css(hideRules); // hide annoying popup's backdrop
        }

        function initCordovaPlugins() {
            const log = $log.get("bootstrap#initCordovaPlugins");
            log.info("init cordova plugins");

            // first of all let's schedule the disable of ripple's annoying popup 
            // that tells us about unregistered plugins
            if (isRippleEmulator()) $timeout(disableRipplePopup);

            // Show/Hide keyboard accessory bar
            if (window.cordova && window.cordova.plugins.Keyboard) {
                cordova.plugins.Keyboard.hideKeyboardAccessoryBar(ionic.Platform.isAndroid());
            }
            // necessary to set fullscreen on Android in order for android:softinput=adjustPan to work
            if (ionic.Platform.isAndroid()) {
                if (window.StatusBar) window.StatusBar.styleDefault();
                //ionic.Platform.isFullScreen = true;
            }
            // local notification 
            synchronizationNotificationService.prepareNotificationFeature();
        }

        /**
            * Handles Android's softinput covering inputs that are close to the bottom of the screen.
            */
        function adjustAndroidSoftInput() {
            window.adjustAndroidSoftInput = adjustAndroidSoftInput;
            if (!ionic.Platform.isAndroid()) return;

            // native.showkeyboard callback
            // e contains keyboard height
            window.addEventListener("native.showkeyboard", e => {
                $timeout(() => {
                    const focusedElement = document.activeElement;
                    if (!focusedElement) return;

                    // no need to subtract e.keyboardHeight: by this point window.innerHeight is the viewport's height which is equal to keyboard's top's position
                    const keyBoardTopPosition = window.innerHeight;
                    const rect = focusedElement.getBoundingClientRect();
                    const elementBottomPosition = rect.bottom;

                    // if input is hidden by keyboard (position is calculated top to bottom)
                    if (keyBoardTopPosition < elementBottomPosition) {
                        // scroll with animation
                        const scrollOffsetY = elementBottomPosition - keyBoardTopPosition;
                        $ionicScrollDelegate.scrollBy(0, scrollOffsetY, true);
                    }
                }, 0, false);
            });

            window.addEventListener('native.hidekeyboard', e => {
                // remove focus from activeElement 
                // which is naturally an input since the nativekeyboard is hiding
                const focusedElement = document.activeElement;
                if (focusedElement) focusedElement.blur();
                // resize scroll after keyboard is gone
                $timeout(() => $ionicScrollDelegate.resize(), 0, false);
            });
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
                const serverurl = localStorageService.get("settings:serverurl");
                if (!!serverurl) {
                    return;
                }
                // prevent state change
                event.preventDefault();
                // go to settings instead
                routeService.go("settings");
            });

            document.addEventListener("resume", initContext, false);

        }

        function loadInitialState() {
            initialRouterService.doInit();
        }

        function hideSplashScreen() {
            return $timeout(() => {
                if ($cordovaSplashscreen && angular.isFunction($cordovaSplashscreen.hide)) $cordovaSplashscreen.hide();
            }, 1000);
        }

        $ionicPlatform.ready(() => {
            // loading eventual db stored values into context
            initContext().then(() => {
                adjustAndroidSoftInput();
                attachEventListeners();
                initCordovaPlugins();
                return loadInitialState();
            })
                .then(hideSplashScreen); // 1 second delay to prevent blank screen right after hiding the splash screen (empirically determined)
        });
    }
])
//#endregion

//#region App.config
    .config([
        "$stateProvider", "$urlRouterProvider", "$logProvider", "$ionicConfigProvider", "$httpProvider", function ($stateProvider, $urlRouterProvider, $logProvider, $ionicConfigProvider, $httpProvider) {

            // center page titles
            $ionicConfigProvider.navBar.alignTitle("center");

            $httpProvider.useApplyAsync(true);
            $ionicConfigProvider.views.transition('none');

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
                // support
                .state("main.support", {
                    url: "/support",
                    views: {
                        'main': {
                            templateUrl: "Content/Mobile/templates/support.html",
                            controller: "SupportController"
                        }
                    }
                })
               .state('main.crudlist', {
                   url: "/crudlist",
                   cache: false,
                   views: {
                       'main': {
                           templateUrl: "Content/Mobile/templates/crudlist.html",
                           controller: 'CrudListController'
                       }
                   }
               })
                .state('main.crudlist.search', {
                    url: "/crudlistsearch",
                    views: {
                        'main@main': {
                            templateUrl: "Content/Mobile/templates/crudlistsearch.html",
                            controller: 'CrudListSearchController'
                        }
                    }
                })
                .state('main.cruddetail', {
                    url: "/cruddetail",
                    views: {
                        'main': {
                            templateUrl: "Content/Mobile/templates/crud_detail.html",
                            controller: 'CrudDetailController'
                        }

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
                    cache: false,
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

        }
    ]);
//#endregion
