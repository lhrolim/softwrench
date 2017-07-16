function HomeController($scope, $http, $location, $templateCache, $rootScope, $timeout, $log, $compile, contextService, menuService, i18NService, alertService) {
    "ngInject";
    $scope.$name = 'HomeController';

    function initController() {
        var log = $log.getInstance('home.js#initController');

        var redirectUrl = url(homeModel.Url);
        i18NService.load(homeModel.I18NJsons, userLanguage);


        var sessionRedirectURL = sessionStorage.swGlobalRedirectURL;
        if (sessionRedirectURL != null && ((redirectUrl.indexOf("popupmode=browser") == -1) && (redirectUrl.indexOf("MakeSWAdmin") == -1))) {
            redirectUrl = sessionRedirectURL;
        }
        var currentModule = contextService.currentModule();
        if (currentModule == undefined || currentModule == "null") {
            //sessionstorage is needed in order to avoid F5 losing currentmodule
            log.info('switching to home module {0}'.format(homeModel.InitialModule));
            contextService.insertIntoContext("currentmodule", homeModel.InitialModule, false);
        }
        $http({
            method: "GET",
            url: redirectUrl,
            cache: $templateCache
        })
            .success(function (result) {
                $scope.$parent.includeURL = contextService.getResourceUrl(result.redirectURL);
                $scope.$parent.resultData = result.resultObject;
                $scope.$parent.resultObject = result;
                //            if (nullOrUndef($rootScope.currentmodule) && !nullOrUndef(currentModule) && currentModule != "") {
                //                $rootScope.currentmodule = currentModule;
                //            }

                $scope.$emit('sw_indexPageLoaded', redirectUrl);
                $scope.$emit('sw_titlechanged', result.title);

                if (homeModel.Message != undefined) {
                    /*if (homeModel.MessageType == 'error') {
                        var error = { errorMessage: homeModel.Message }
    
                        $timeout(function () {                        
                            $rootScope.$broadcast('sw_ajaxerror', error);
                        }, 1000);
                        
                    } else {*/
                    alertService.success(homeModel.Message, false);
                    //}
                    homeModel.Message = null;
                }
            });
    }



    $scope.onTemplateLoad = function (event) {
        $timeout(function () {
            $scope.$emit('ngLoadFinished');
            var windowTitle = homeModel.WindowTitle;
            if (!nullOrUndef(windowTitle)) {
                window.document.title = windowTitle;
            }
            var content = angular.element('#headerline');
            $compile(content.contents())($scope);
        });
    };

    initController();
}