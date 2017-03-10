(function (angular) {
    "use strict";


    function HomeController($scope, $http, $templateCache, $rootScope, $timeout, $log, $q, contextService, i18NService, alertService, statuscolorService,
        classificationColorService, historyService, configurationService, localStorageService, userPreferencesService, restService, schemaCacheService, logoutService, dynamicScriptsCacheService) {
        const APP_VERION = 'sw_system_version_key';
        $scope.$name = 'HomeController';

        const log = $log.get("HomeController#init", ["init", "navigation", "route"]);




        function initController() {

            log.debug("init home controller");

            var localHomeModel = homeModel;

            if (localHomeModel.Error) {
                handleError(localHomeModel);
            }

            const configsJSON = localHomeModel.ConfigJSON;
            const userJSON = localHomeModel.UserJSON;

            if (nullOrEmpty(configsJSON) || nullOrEmpty(userJSON) || contextService.get("sw:changepasword")) {
                contextService.deleteFromContext("sw:changepasword");
                //this means user tried to hit back button after logout
                return logoutService.logout();
            }

            //parsing only if if needed
            const config = JSON.parse(configsJSON);
            const user = JSON.parse(userJSON);

            contextService.loadUserContext(user);
            contextService.loadConfigs(config);

            $scope.mainlogo = config.logo;
            $scope.myprofileenabled = config.myProfileEnabled;

            if (!sessionStorage["ctx_loggedin"]) {
                //if the session storage flag was null it meanas login has just happened. Otherwise we´re talking about a browser refresh
                $rootScope.$broadcast(JavascriptEventConstants.Login);
                dynamicScriptsCacheService.syncWithServerSideScripts();
            }

            //workaround for knowing where the user is already loggedin
            sessionStorage["ctx_loggedin"] = true;



            if (localHomeModel.RouteInfo) { // store route info on local storage
                localStorageService.put(historyService.routeInfoKey, localHomeModel.RouteInfo);
            }

            var redirectUrl = url(localHomeModel.Url);

            if (localHomeModel.RouteListInfo) { // it's a list route -> mark pre filters and page size
                redirectUrl += "&SearchDTO[addPreSelectedFilters]=true";
                const pageSize = userPreferencesService.getSchemaPreference("pageSize", localHomeModel.RouteListInfo.ApplicationName, localHomeModel.RouteListInfo.Schemaid);
                if (pageSize) {
                    redirectUrl += `&SearchDTO[pageSize]=${pageSize}`;
                }
            }

            const menuModel = JSON.parse(localHomeModel.MenuJSON);

            //force initialization of schema cache for this given app
            schemaCacheService.getCachedSchema(localHomeModel.ApplicationName, localHomeModel.SchemaId);
            i18NService.load(localHomeModel.I18NJsons, userLanguage);
            statuscolorService.load(localHomeModel.StatusColorJson, localHomeModel.StatusColorFallbackJson);

            classificationColorService.load(localHomeModel.ClassificationColorJson);

            $scope.$emit("sw_loadmenu", menuModel);
            return handleRedirect(redirectUrl, localHomeModel);

        }

        function handleError(localHomeModel) {
            if (localHomeModel.Error.ErrorStack) {
                const errorData = {
                    errorMessage: localHomeModel.Error.ErrorMessage,
                    errorStack: localHomeModel.Error.ErrorStack,
                    errorType: localHomeModel.Error.ErrorType,
                    outlineInformation: localHomeModel.Error.OutlineInformation
                }
                alertService.notifyexception(errorData);
            } else {
                alertService.notifymessage("error", localHomeModel.Error.ErrorMessage);
            }
        }

        function handleRedirect(redirectUrl, localHomeModel) {
            const sessionRedirectUrl = contextService.fetchFromContext("swGlobalRedirectURL", false, false);
            if (sessionRedirectUrl != null && redirectUrl && ((redirectUrl.indexOf("popupmode=browser") < 0) && (redirectUrl.indexOf("MakeSWAdmin") < 0)) && !homeModel.FromRoute) {
                redirectUrl = sessionRedirectUrl;
            }
            const locationUrl = historyService.getLocationUrl();
            if (locationUrl) {
                redirectUrl = locationUrl;
            } else {
                historyService.addToHistory(redirectUrl, false, true);
            }

            //Check if the user is sysadmin and the application version has changed since last login for this user
            if (contextService.HasRole(["sysadmin"]) && appVersionChanged(localHomeModel.ApplicationVersion)) {
                redirectUrl = restService.getActionUrl("DeployValidation", "Index", null);
            }

            if (!redirectUrl) {
                return $q.when(null);
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
        $scope.$on("$locationChangeSuccess", function (event, newUrl, oldUrl) {
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

        $scope.onTemplateLoad = function () {
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

    app.controller("HomeController", ["$scope", "$http", "$templateCache", "$rootScope", "$timeout", "$log", "$q", "contextService", "i18NService", "alertService", "statuscolorService", "classificationColorService", "historyService", "configurationService", "localStorageService", "userPreferencesService", "restService", "schemaCacheService", "logoutService", "dynamicScriptsCacheService", HomeController]);
    window.HomeController = HomeController;

})(angular);