(function (angular) {
    "use strict";

    function HomeController($scope, $http, $templateCache, $rootScope, $timeout, $location, $log, contextService, menuService, i18NService, alertService, statuscolorService, redirectService, classificationColorService, historyService, configurationService, localStorageService, userPreferencesService) {

        $scope.$name = "HomeController";

        function initController() {
            //workaround for knowing where the user is already loggedin
            sessionStorage["ctx_loggedin"] = true;
            if (homeModel.RouteInfo) { // store route info on local storage
                localStorageService.put(historyService.routeInfoKey, homeModel.RouteInfo);
            }

            var redirectUrl = url(homeModel.Url);

            if (homeModel.RouteListInfo) { // it's a list route -> mark pre filters and page size
                redirectUrl += "&SearchDTO[AddPreSelectedFilters]=true";
                const pageSize = userPreferencesService.getSchemaPreference("pageSize", homeModel.RouteListInfo.ApplicationName, homeModel.RouteListInfo.Schemaid);
                if (pageSize) {
                    redirectUrl += `&SearchDTO[pageSize]=${pageSize}`;
                }
            }

            const menuModel = JSON.parse(homeModel.MenuJSON);

            i18NService.load(homeModel.I18NJsons, userLanguage);
            statuscolorService.load(homeModel.StatusColorJson);
            statuscolorService.loadFallback(homeModel.StatusColorFallbackJson);
            classificationColorService.load(homeModel.ClassificationColorJson);

            $scope.$emit("sw_loadmenu", menuModel);

            const sessionRedirectUrl = contextService.fetchFromContext("swGlobalRedirectURL", false, false);
            if (sessionRedirectUrl != null && ((redirectUrl.indexOf("popupmode=browser") < 0) && (redirectUrl.indexOf("MakeSWAdmin") < 0)) && !homeModel.FromRoute) {
                redirectUrl = sessionRedirectUrl;
            }

            const locationUrl = historyService.getLocationUrl();
            if (locationUrl) {
                redirectUrl = locationUrl;
            } else {
                historyService.addToHistory(redirectUrl, false, true);
            }
            //        if (sessionStorage.currentmodule != undefined && sessionStorage.currentmodule != "null") {
            //            //sessionstorage is needed in order to avoid F5 losing currentmodule
            //            $rootScope.currentmodule = sessionStorage.currentmodule;
            //        }
            redirect(redirectUrl);
        }

        function redirect(redirectUrl, avoidTemplateCache) {
            const parameters = {
                method: "GET",
                url: redirectUrl
            }
            if (!avoidTemplateCache) {
                parameters.cache = $templateCache;
            }

            $http(parameters).then(function (response) {
                // updates the configs on page load
                configurationService.updateConfigurations();

                const result = response.data;

                $scope.$parent.includeURL = contextService.getResourceUrl(result.redirectURL);
                $scope.$parent.resultData = result.resultObject;
                $scope.$parent.resultObject = result;
                // if (nullOrUndef($rootScope.currentmodule) && !nullOrUndef(currentModule) && currentModule != "") {
                // $rootScope.currentmodule = currentModule;
                // }

                $scope.$emit('sw_indexPageLoaded', redirectUrl);
                $scope.$emit('sw_titlechanged', result.title);
                if (!angular.isUndefined(homeModel.Message)) {
                    const messageType = homeModel.MessageType || "success";
                    alertService.notifymessage(messageType, homeModel.Message);
                    homeModel.Message = null;
                }
            });
        }

        // listen to a location change to redirect on browser back and forward navigation
        $rootScope.$on("$locationChangeSuccess", function (event, newUrl, oldUrl) {
            if (newUrl === oldUrl || oldUrl && oldUrl.endsWith(location.pathname)) {
                return;
            }

            // workaround - if the location change was originated by adding a url on history (causes a hash change on url) ignores this redirect
            if (historyService.wasLocationUpdatedByService()) {
                historyService.resetLocationUpdatedByService();
                return;
            }

            const redirectUrl = historyService.getLocationUrl();
            if (!redirectUrl) {
                return;
            }
            const log = $log.getInstance("HomeController#locationChangeSuccess");
            log.debug("Redirecting to ({0}) as a browser navigation".format(redirectUrl));
            redirect(redirectUrl, true);
        });

        $scope.onTemplateLoad = function (event) {
            $timeout(function () {
                $scope.$emit("ngLoadFinished");
                const windowTitle = homeModel.WindowTitle;
                if (!nullOrUndef(windowTitle)) {
                    window.document.title = windowTitle;
                }
            });
        };

        initController();
    }

    app.controller("HomeController", ["$scope", "$http", "$templateCache", "$rootScope", "$timeout", "$location", "$log", "contextService", "menuService", "i18NService", "alertService", "statuscolorService", "redirectService", "classificationColorService", "historyService", "configurationService", "localStorageService", "userPreferencesService", HomeController]);
    window.HomeController = HomeController;

})(angular);