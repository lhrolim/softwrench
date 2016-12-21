(function (angular) {
    "use strict";

    function HomeController($scope, $http, $templateCache, $rootScope, $timeout, $location, $log, contextService, menuService, i18NService, alertService, statuscolorService, redirectService, classificationColorService, historyService, configurationService, localStorageService, userPreferencesService, restService) {
        const APP_VERION = 'sw_system_version_key';
        $scope.$name = 'HomeController';

        const log = $log.get("HomeController#init", ["init", "navigation", "route"]);
        

        function initController() {
        
            log.debug("init home controller");
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

            //Check if the user is sysadmin and the application version has changed since last login for this user
            if (contextService.HasRole(["sysadmin"]) && appVersionChanged(homeModel.ApplicationVersion)) {
                redirectUrl = restService.getActionUrl("DeployValidation", "Index", null);
            }

            return redirect(redirectUrl);
        }

        function redirect(redirectUrl, avoidTemplateCache) {
            const parameters = {
                method: "GET",
                url: redirectUrl
            }
            if (!avoidTemplateCache) {
                parameters.cache = $templateCache;
            }

            log.info(`getting crud data at ${redirectUrl}`);
            return $http(parameters).then(function (response) {
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
                $scope.$emit(JavascriptEventConstants.TitleChanged, result.title);
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
                return false;
            }

            // workaround - if the location change was originated by adding a url on history (causes a hash change on url) ignores this redirect
            if (historyService.wasLocationUpdatedByService()) {
                historyService.resetLocationUpdatedByService();
                return false;
            }

            const redirectUrl = historyService.getLocationUrl();
            if (!redirectUrl) {
                return false;
            }
            const log = $log.getInstance("HomeController#locationChangeSuccess");
            log.debug("Redirecting to ({0}) as a browser navigation".format(redirectUrl));
            return redirect(redirectUrl, true);
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

        function appVersionChanged(currentVersion) {
            const localVersion = localStorageService.get(APP_VERION);
            if (!currentVersion || (localVersion && currentVersion.equalsIc(localVersion))) {
                return false;
            } else {
                localStorageService.put(APP_VERION, currentVersion);
                return true;
            }
        }

        initController();
    }

    app.controller("HomeController", ["$scope", "$http", "$templateCache", "$rootScope", "$timeout", "$location", "$log", "contextService", "menuService", "i18NService", "alertService", "statuscolorService", "redirectService", "classificationColorService", "historyService", "configurationService", "localStorageService", "userPreferencesService", "restService", HomeController]);
    window.HomeController = HomeController;

})(angular);