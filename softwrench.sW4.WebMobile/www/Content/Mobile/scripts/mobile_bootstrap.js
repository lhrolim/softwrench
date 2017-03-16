//#region 'deviceready' listener
document.addEventListener("deviceready", function () {
    // retrieve the DOM element that had the ng-app attribute
    // bootstrap angular app "softwrench" programatically
    angular.bootstrap(document.body, ["softwrench"]);
}, false);
//#endregion

var crudinput = "";
crudinput += "<div class=\"list\">";
crudinput += "    <messagesection><\/messagesection>";
crudinput += "";
crudinput += "    <crud-input-fields schema=\"schema\" displayables=\"displayables\" all-displayables=\"allDisplayables\" datamap=\"datamap\"><\/crud-input-fields>";
crudinput += "";
crudinput += "    <div>";
crudinput += "        <command-bar schema=\"schema\" datamap=\"datamap\" position=\"mobile.fab\" label=\"Actions\"><\/command-bar>";
crudinput += "    <\/div>";
crudinput += "<\/div>";


var crudlist = "";
crudlist += "<ion-view cache-view=\"false\">";
crudlist += "";
crudlist += "    <ion-nav-buttons side=\"left\">";
crudlist += "        <button class=\"button-icon show-phone\" menu-toggle=\"left\"><i class=\"fa fa-bars\"><\/i><\/button>";
crudlist += "    <\/ion-nav-buttons>";
crudlist += "";
crudlist += "    <ion-nav-title>";
crudlist += "        <div ng-show=\"!isSearching()\">{{gridTitle()}}";
crudlist += "            <div class=\"buttons\" style=\"position: absolute; right: 5px; top:5px\" ng-show=\"isList()\">";
crudlist += "                <button class=\"button\" ng-click=\"showFilterOptions($event)\"><i class=\"fa fa-fa-ellipsis-v\"><\/i><\/button>";
crudlist += "            <\/div>";
crudlist += "        <\/div>";
crudlist += "";
crudlist += "        <div class=\"list-search-bar item-input-inset\" ng-show=\"isSearching() && isList()\">";
crudlist += "            <label class=\"item-input-wrapper\">";
crudlist += "                <i class=\"placeholder-icon fa fa-search\"><\/i>";
crudlist += "                <input type=\"search\" class=\"ion-autocomplete-search\" ng-model=\"quickSearch.value\" ng-change=\"filter()\" placeholder=\"Quick Search\" ng-model-options=\"{ debounce: 500 }\"\/>";
crudlist += "                <button class=\"button-icon search-disable-button\" ng-touchstart=\"disableSearch(true)\"><i class=\"fa fa-times-circle\"><\/i><\/button>";
crudlist += "            <\/label>            ";
crudlist += "        <\/div>";
crudlist += "    <\/ion-nav-title>";
crudlist += "";
crudlist += "   ";
crudlist += "    <ion-nav-buttons side=\"right\">";
crudlist += "        <button class=\"button-icon\" ng-show=\"isList() && isSearching()\" ng-touchstart=\"goToAdvancedFilter()\"><i class=\"fa fa-sliders\"><\/i><\/button>";
crudlist += "        <!--this ng-show is needed due to a possible bug on ionic\/ angular ui-router where the button shows on detail-->";
crudlist += "        <button class=\"button-icon\" ng-show=\"isList() && !isSearching()\" ng-touchstart=\"enableSearch()\"><i class=\"fa fa-search\"><\/i><\/button>";
crudlist += "    <\/ion-nav-buttons>";
crudlist += "";
crudlist += "    <ion-content has-header=\"true\" has-bouncing=\"false\" on-swipe-right=\"toggleMenu()\" >";
crudlist += "";
crudlist += "        <div class=\"row\" ng-if=\"crudlist.items.length == 0\">";
crudlist += "            <div class=\"col col-center text-center\">";
crudlist += "                <h4 class=\"gray\">No Entries Found<\/h4>";
crudlist += "            <\/div>";
crudlist += "        <\/div>";
crudlist += "";
crudlist += "        <ion-refresher pulling-text=\"Pull to Sync\" on-refresh=\"fullSync()\" pulling-icon=\"fa fa-refresh\" spinner=\"none\"> <\/ion-refresher>";
crudlist += "";
crudlist += "        <ion-list class=\"has-header crud-list\" ng-if=\"crudlist.items.length >0\" >";
crudlist += "            <!--generatedRowStamp is a transient column generated on persistence (persistence.store.sql) entity hydratation to allow multiple rows with same id (left joins...) -->";
crudlist += "            <ion-item class=\"crud-item\"";
crudlist += "                      ng-repeat=\"item in crudlist.items track by item.generatedRowStamp\"";
crudlist += "                      item=\"item\"";
crudlist += "                      on-hold=\"showGridItemOptions($event,item)\">";
crudlist += "                <div class=\"row\" ng-click=\"openDetail(item)\">";
crudlist += "                    <div class=\"col col-25 crud-icon\">";
crudlist += "                        <crud-icon item=\"item\"><\/crud-icon>";
crudlist += "                    <\/div>";
crudlist += "                    <div class=\"col\">";
crudlist += "                        <ul>";
crudlist += "                            <li class=\"featured\">{{itemFeatured(item.datamap)}}&ensp;<span class=\"details\"><i class=\"fa fa-chevron-right\"><\/i><\/span><\/li>";
crudlist += "                            <li class=\"title\">{{itemTitle(item.datamap)}}<\/li>";
crudlist += "                            <li class=\"subtitle\">{{itemSubTitle(item.datamap)}}<\/li>";
crudlist += "                            <li class=\"excerpt\">{{itemExcerpt(item.datamap)}}<\/li>";
crudlist += "                        <\/ul>";
crudlist += "                    <\/div>";
crudlist += "                <\/div>";
crudlist += "            <\/ion-item>";
crudlist += "";
crudlist += "        <\/ion-list>";
crudlist += "";
crudlist += "        <ion-infinite-scroll ng-if=\"crudlist.moreItemsAvailable\" on-infinite=\"loadMore()\" distance=\"10%\"><\/ion-infinite-scroll>";
crudlist += "";
crudlist += "    <\/ion-content>";
crudlist += "";
crudlist += "    <button class=\"button button-float\" ng-click=\"createItem()\" ng-if=\"createEnabled()\">";
crudlist += "        <i class=\"fa fa-plus\"><\/i>";
crudlist += "    <\/button>";
crudlist += "";
crudlist += "<\/ion-view>";
crudlist += "";


var strVar = "";
strVar += "<ion-view>";
strVar += "";
strVar += "    <ion-nav-buttons side=\"left\">";
strVar += "        <button class=\"button-icon\" menu-toggle=\"left\"><i class=\"fa fa-bars\"><\/i><\/button>";
strVar += "    <\/ion-nav-buttons>";
strVar += "    ";
strVar += "    <ion-nav-title>";
strVar += "        <div>{{title()}}<\/div>";
strVar += "    <\/ion-nav-title>";
strVar += "    ";
strVar += "    ";
strVar += "    <ion-header-bar class=\"bar-subheader bar-dark\" align-title=\"left\">";
strVar += "";
strVar += "        <div class=\"buttons\" style=\"padding-left: 5px\">";
strVar += "            <button class=\"button\" ng-show=\"shouldShowBack()\" ng-click=\"navigateBack()\"><i class=\"fa fa-arrow-left\"><\/i>&ensp;Back<\/button>";
strVar += "            <button class=\"button\" ng-show=\"hasDirtyChanges() && !shouldShowWizardBack()\" ng-click=\"cancelChanges()\"><i class=\"fa fa-times-circle\"><\/i>&ensp;Cancel<\/button>";
strVar += "            <button class=\"button\" ng-show=\"shouldShowWizardBack()\" ng-click=\"wizardNavigateBack()\"><i class=\"fa fa-arrow-left\"><\/i>&ensp;Back<\/button>";
strVar += "        <\/div>";
strVar += "";
strVar += "        <!-- empty title to position the buttons correctly -->";
strVar += "        <h1 class=\"title\"><\/h1>";
strVar += "";
strVar += "        <div class=\"buttons\" style=\"white-space: nowrap\">";
strVar += "            <button class=\"button navigate\" ng-click=\"navigatePrevious()\" ng-if=\"showNavigation() && hasPreviousItem()\"><i class=\"fa fa-chevron-left\"><\/i><\/button>";
strVar += "            <button class=\"button navigate\" ng-click=\"navigateNext()\" ng-if=\"showNavigation() && hasNextItem()\"><i class=\"fa fa-chevron-right\"><\/i><\/button>";
strVar += "            <div class=\"seperator\" ng-if=\"!hasDirtyChanges() && hasAnyComposition() && showNavigation()\"><\/div>";
strVar += "            <button class=\"button\" ng-click=\"expandCompositions($event)\" ng-if=\"!hasDirtyChanges() && hasAnyComposition()\"><i class=\"fa fa-th\"><\/i><\/button>";
strVar += "            <button class=\"button\" ng-click=\"saveChanges()\" ng-if=\"hasDirtyChanges() && !shouldShowWizardForward()\"><i class=\"fa fa-check\"><\/i>&ensp;Save<\/button>";
strVar += "            <button class=\"button\" ng-click=\"wizardNavigateForward()\" ng-if=\"shouldShowWizardForward()\"><i class=\"fa fa-arrow-right\"><\/i>&ensp;Next<\/button>";
strVar += "        <\/div>";
strVar += "";
strVar += "    <\/ion-header-bar>";
strVar += "";
strVar += "    <ion-content class=\"crud-details\" has-header=\"true\" has-subheader=\"true\" has-bouncing=\"false\" ";
strVar += "                 on-swipe-left=\"onSwipeLeft()\" on-swipe-right=\"onSwipeRight()\" on-scroll=\"onScroll()\" delegate-handle=\"detailHandler\">";
strVar += "        <div class=\"crud-title\" ng-if=\"isOnMainTab()\">";
strVar += "            <div class=\"row\" on-hold=\"showDirtyOptions($event)\">";
strVar += "                <div class=\"col col-25 crud-icon\">";
strVar += "                    <crud-icon item=\"item\" isdetail=\"true\" datamap=\"datamap\"><\/crud-icon>";
strVar += "                <\/div>";
strVar += "                <div class=\"col\">";
strVar += "                    <ul>";
strVar += "                        <li class=\"featured\">{{detailFeatured()}}<\/li>";
strVar += "                        <li class=\"title\">{{detailTitle()}}<\/li>";
strVar += "                        <li class=\"subtitle\">{{detailSubTitle()}}<\/li>";
strVar += "                        <li class=\"excerpt\">{{detailSummary()}}<\/li>";
strVar += "                    <\/ul>";
strVar += "                <\/div>";
strVar += "            <\/div>";
strVar += "        <\/div>";
strVar += "";
strVar += "        <div div class=\"crud-title h4\" ng-click=\"loadMainTab()\" ng-if=\"!isOnMainTab()\">";
strVar += "            {{tabTitle()}}";
strVar += "        <\/div>";
strVar += "";
strVar += "        <div class=\"crud-description\">";
strVar += "            <div ng-if=\"hasProblems()\" class=\"problem-description\">";
strVar += "                <i class=\"fa fa-exclamation-triangle\"><\/i>&ensp;{{lastProblemMesage()}}";
strVar += "            <\/div>";
strVar += "        <\/div>";
strVar += "";
strVar += "        <ion-nav-view name=\"body\" class=\"list\" style=\"height: initial\"><\/ion-nav-view>";
strVar += "";
strVar += "    <\/ion-content>";
strVar += "";
strVar += "    <div ng-if=\"!!compositionListSchema\" ng-show=\"!hasDirtyChanges()\">";
strVar += "        <command-bar schema=\"compositionListSchema\" datamap=\"datamap\" position=\"mobile.composition\" label=\"Actions\"><\/command-bar>";
strVar += "    <\/div>";
strVar += "<\/ion-view>";
strVar += "";


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

        function initContext() {
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
                            template: crudlist,
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
                            template: strVar,
                            controller: 'CrudDetailController'
                        }

                    }
                })
                .state('main.cruddetail.maininput', {
                    url: "/crudinput",
                    views: {
                        'body': {
                            template: crudinput,
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
