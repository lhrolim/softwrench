var app = angular.module('sw_layout', ['pasvaz.bindonce', 'angularTreeview', 'ngSanitize']).config(function ($controllerProvider) {
//    $controllerProvider.allowGlobals()
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

app.filter('html', ['$sce', function ($sce) {
        return function (text) {
            return $sce.trustAsHtml(text);
        };
}]);



app.directive('onFinishRender', function ($timeout) {
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




function LayoutController($scope, $http, $log, $templateCache, $rootScope, $timeout, fixHeaderService, redirectService, i18NService, menuService, contextService, $location, $window, logoutService, spinService, schemaCacheService) {
    "ngInject";

    $scope.$name = 'LayoutController';

    schemaCacheService.wipeSchemaCacheIfNeeded();

    $rootScope.$on('sw_ajaxinit', function (ajaxinitevent) {
        var savingMain = true === $rootScope.savingMain;
        spinService.start({ savingDetail: savingMain });
    });

    $rootScope.$on('sw_ajaxend', function (data) {
        spinService.stop();
        $rootScope.savingMain = undefined;

    });

    $rootScope.$on('sw_ajaxerror', function (data) {
        spinService.stop();
        $rootScope.savingMain = undefined;

    });

    $scope.$on('sw_titlechanged', function (titlechangedevent, title) {
        $scope.title = title;
    });

    $scope.$on('ngLoadFinished', function (ngLoadFinishedEvent) {
        $('[rel=tooltip]').tooltip({ container: 'body' });
        menuService.adjustHeight();
    });

    $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {
        $('[rel=tooltip]').tooltip({ container: 'body' });

        var sidebarWidth = $('.col-side-bar').width();
        if (sidebarWidth != null) {
            $('.col-main-content').css('margin-left', sidebarWidth);
        }
    });

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.goToApplicationView = function (applicationName, schemaId, mode, title, parameters, target) {
        menuService.setActiveLeaf(target);
        redirectService.goToApplicationView(applicationName, schemaId, mode, title, parameters);
    };

    $scope.doAction = function (title, controller, action, parameters, target) {
        menuService.setActiveLeaf(target);
        redirectService.redirectToAction(title, controller, action, parameters);
    };

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


    $scope.AjaxResult = function (result) {
        if ($scope.resultObject.requestTimeStamp > result.requestTimeStamp) {
            //ignoring intermediary request
            return;
        }

        var log = $log.getInstance('layoutcontroller#AjaxResult');
        var newUrl = url(result.redirectURL);
        if ($scope.includeURL != newUrl) {
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

    $scope.logout = function () {
        logoutService.logout("manual");
    };

    $scope.$on('sw_goToApplicationView', function (event, data) {
        if (data != null) {
            $scope.goToApplicationView(data.applicationName, data.schemaId, data.mode, data.title, data.parameters);
        }
    });

    $scope.$on('sw_indexPageLoaded', function (event, url) {
        if (url != null && $rootScope.menu) {
            menuService.setActiveLeafByUrl($rootScope.menu, url);
        }
    });

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
        contextService.insertIntoContext("allowedfiles", config.allowedFileTypes);
        
        $rootScope.clientName = config.clientName;
        $rootScope.environment = config.environment;
        $rootScope.isLocal = config.isLocal;
        $rootScope.i18NRequired = config.i18NRequired;

        $scope.mainlogo = config.logo;
        $scope.myprofileenabled = config.myProfileEnabled;
        var popupMode = GetPopUpMode();
        $scope.popupmode = popupMode;
        if (popupMode != "none") {
            return;
        }
        $http({
            method: "GET",
            url: url("/api/menu?" + platformQS()),
            cache: $templateCache
        })
        .success(function (menuAndNav) {
            $rootScope.menu = menuAndNav.menu;
            $scope.menu = menuAndNav.menu;
            $scope.isSysAdmin = menuAndNav.isSysAdmin;
            $scope.isClientAdmin = menuAndNav.isClientAdmin;
          
            $('.hapag-body').addClass('hapag-body-loaded');
        })
        .error(function (data) {
            $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
        });
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    initController();
}

