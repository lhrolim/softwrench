﻿var swdb;

//window.ionic.Platform.ready(function () {
//    //initing the softwrench application programatically instead of using ng-app for the sake of persistence.js ripple/device compatibility
//    angular.bootstrap(document, ['softwrench']);
//});


var mobileServices = angular.module('sw_mobile_services', ['webcommons_services']);
var softwrench = angular.module('softwrench', ['ionic', 'ngCordova', 'sw_mobile_services', 'webcommons_services'])



.run(function ($ionicPlatform, swdbDAO, loginService, contextService,menuModelService,metadataModelService, $state) {

    initContext();

    $ionicPlatform.ready(function () {


//        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
//            WebView.setWebContentsDebuggingEnabled(true);
//        }

        initCordovaPlugins();
      

        var isCookieAuthenticated = loginService.checkCookieCredentials();
        if (isCookieAuthenticated) {
            $state.go('main.home');
            return;
        }
        $state.go('login');
    });



    function initContext() {
        swdbDAO.init();
        menuModelService.initAndCacheFromDB();
        metadataModelService.initAndCacheFromDB();
        swdbDAO.findAll("Settings").success(function(settings) {
            if (settings.length == 0) {
                var ob = entities.Settings;
                swdbDAO.save(new ob()).success(function() {
                    contextService.insertIntoContext("settings", settings);
                });
            } else {
                contextService.insertIntoContext("settings", settings[0]);
                contextService.insertIntoContext("serverurl", settings[0].serverurl);
            }
            
        });
        
    }

    function initCordovaPlugins() {
        
        // Hide the accessory bar by default (remove this to show the accessory bar above the keyboard
        // for form inputs)
        if (window.cordova && window.cordova.plugins.Keyboard) {
            cordova.plugins.Keyboard.hideKeyboardAccessoryBar(true);
        }
        //        if (window.StatusBar) {
        //            // org.apache.cordova.statusbar required
        //            StatusBar.styleDefault();
        //        }
    };

})

.config(function ($stateProvider, $urlRouterProvider) {

    // Ionic uses AngularUI Router which uses the concept of states
    // Learn more here: https://github.com/angular-ui/ui-router
    // Set up the various states which the app can be in.
    // Each state's controller can be found in controllers.js
    $stateProvider
        .state('login', {
            url: '/login',
            templateUrl: 'Content/Mobile/templates/login.html',
            controller: 'LoginController'
        })

        .state('settings', {
            url: '/settings',
            templateUrl: 'Content/Mobile/templates/settings.html',
            controller: 'SettingsController'
        })

        // setup an abstract state for the tabs directive
          .state('main', {
              url: "/main",
              templateUrl: "Content/Mobile/templates/main.html",
              abstract: true,
              controller: 'MainController'
          })


        .state('main.home', {
            url: '/home',
            views: {
                'main': {
                    templateUrl: 'Content/Mobile/templates/home.html',
                    controller: 'HomeController'
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


    // if none of the above states are matched, use this as the fallback
    $urlRouterProvider.otherwise('/main/home');

});