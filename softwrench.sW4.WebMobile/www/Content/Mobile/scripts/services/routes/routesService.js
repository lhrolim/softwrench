(function (mobileServices) {
    "use strict";

    mobileServices.factory('routeService', ["$state", "contextService", "settingsService", "localStorageService", "loadingService", "$rootScope", "routeConstants",
        function ($state, contextService, settingsService, localStorageService, loadingService, $rootScope, routeConstants) {

            const loginURL = () => settingsService.getServerUrl().then(url => url + "/SignIn/SignInReturningUserData");

            const go = function (stateName, params) {
                // TODO: insert params in the context and recover
                contextService.insertIntoContext("currentstate", stateName);
                return loadingService.showDefault().finally(() => {
                    const isSameState = $state.$current.name === stateName;
                    const stateTransition = $state.go(stateName, params);
                    if (isSameState) {
                        stateTransition.then(state => $rootScope.$broadcast(routeConstants.events.sameStateTransition, state));
                    }
                    loadingService.hide();
                });
            };

            const loadInitialState = function (authenticated) {
                if (!authenticated) {
                    return !localStorageService.get("settings:serverurl") ? this.go("settings") : this.go("login");
                }
                const currentState = contextService.getFromContext("currentstate");
                if (isRippleEmulator() && currentState) {
                    return this.go(currentState);
                }
                return this.go("main.home");
            };

            const api = {
                go,
                loginURL,
                loadInitialState
            }
            return api;

        }]);

})(mobileServices)
