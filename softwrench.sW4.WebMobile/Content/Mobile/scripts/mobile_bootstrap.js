var swdb;
var mobileServices = angular.module('sw_mobile_services', []);
var softwrench = angular.module('softwrench', ['ionic', 'ngCordova', 'sw_mobile_services'])



.run(function ($ionicPlatform, swdbProxy, loginService, $state) {

    function initCordovaPlugins() {
        swdbProxy.init();
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

    //
    //    // Each tab has its own nav history stack:
    //
    
    //
    //    .state('tab.chats', {
    //        url: '/chats',
    //        views: {
    //            'tab-chats': {
    //                templateUrl: 'templates/tab-chats.html',
    //                controller: 'ChatsCtrl'
    //            }
    //        }
    //    })
    //      .state('tab.chat-detail', {
    //          url: '/chats/:chatId',
    //          views: {
    //              'tab-chats': {
    //                  templateUrl: 'templates/chat-detail.html',
    //                  controller: 'ChatDetailCtrl'
    //              }
    //          }
    //      })
    //
    //    .state('tab.friends', {
    //        url: '/friends',
    //        views: {
    //            'tab-friends': {
    //                templateUrl: 'templates/tab-friends.html',
    //                controller: 'FriendsCtrl'
    //            }
    //        }
    //    })
    //      .state('tab.friend-detail', {
    //          url: '/friend/:friendId',
    //          views: {
    //              'tab-friends': {
    //                  templateUrl: 'templates/friend-detail.html',
    //                  controller: 'FriendDetailCtrl'
    //              }
    //          }
    //      })
    //
    //    .state('tab.account', {
    //        url: '/account',
    //        views: {
    //            'tab-account': {
    //                templateUrl: 'templates/tab-account.html',
    //                controller: 'AccountCtrl'
    //            }
    //        }
    //    });

    // if none of the above states are matched, use this as the fallback
    $urlRouterProvider.otherwise('/login');

});