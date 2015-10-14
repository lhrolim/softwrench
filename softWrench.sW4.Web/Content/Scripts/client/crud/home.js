﻿function HomeController($scope, $http, $templateCache, $rootScope, $timeout, contextService, menuService, i18NService, alertService, statuscolorService, redirectService) {

    $scope.$name = 'HomeController';

    function initController() {
        //workaround for knowing where the user is already loggedin
        sessionStorage['ctx_loggedin'] = true;

        var redirectUrl = url(homeModel.Url);

        var menuModel =JSON.parse(homeModel.MenuJSON);

        i18NService.load(homeModel.I18NJsons, userLanguage);
        statuscolorService.load(homeModel.StatusColorJson);


        $scope.$emit("sw_loadmenu", menuModel);

        

        var sessionRedirectURL = contextService.fetchFromContext("swGlobalRedirectURL",false,false);
        if (sessionRedirectURL != null && ((redirectUrl.indexOf("popupmode=browser") < 0) && (redirectUrl.indexOf("MakeSWAdmin") < 0))) {
            redirectUrl = sessionRedirectURL;
        }
        //        if (sessionStorage.currentmodule != undefined && sessionStorage.currentmodule != "null") {
        //            //sessionstorage is needed in order to avoid F5 losing currentmodule
        //            $rootScope.currentmodule = sessionStorage.currentmodule;
        //        }
        $http({
            method: "GET",
            url: redirectUrl,
            cache: $templateCache
        })
        .then(function (response) {
            var result = response.data;

            $scope.$parent.includeURL = contextService.getResourceUrl(result.redirectURL);
            $scope.$parent.resultData = result.resultObject;
            $scope.$parent.resultObject = result;
            // if (nullOrUndef($rootScope.currentmodule) && !nullOrUndef(currentModule) && currentModule != "") {
            // $rootScope.currentmodule = currentModule;
            // }

            $scope.$emit('sw_indexPageLoaded', redirectUrl);
            $scope.$emit('sw_titlechanged', result.title);
            if (!angular.isUndefined(homeModel.Message)) {
                alertService.notifymessage('success', homeModel.Message);
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
        });
    };

    initController();
}

app.controller("HomeController", ["$scope", "$http", "$templateCache", "$rootScope", "$timeout", "contextService", "menuService", "i18NService", "alertService", "statuscolorService", "redirectService", HomeController]);