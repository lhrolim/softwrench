(function (mobileServices) {
    "use strict";



    class routeService {
        constructor($state, contextService, settingsService, localStorageService, loadingService, $rootScope, routeConstants) {
            this.$state = $state;
            this.localStorageService = localStorageService;
            this.contextService = contextService;
            this.settingsService = settingsService;
            this.loadingService = loadingService;
            this.$rootScope = $rootScope;
            this.routeConstants = routeConstants;
            this.loginURL = () => this.settingsService.getServerUrl().then(url => url + "/SignIn/SignInReturningUserData");
        }

        go(stateName, params) {
            // TODO: insert params in the context and recover
            this.contextService.insertIntoContext("currentstate", stateName);
            return this.loadingService.showDefault().finally(() => {
                const isSameState = this.$state.$current.name === stateName;
                const stateTransition = this.$state.go(stateName, params);
                if (isSameState) {
                    stateTransition.then(state => this.$rootScope.$broadcast(this.routeConstants.events.sameStateTransition, state));
                }
                this.loadingService.hide();
            });
        }


        loadInitialState(authenticated) {
            if (!authenticated) {
                return !this.localStorageService.get("settings:serverurl") ? this.go("settings") : this.go("login");
            }
            const currentState = this.contextService.getFromContext("currentstate");
            if (isRippleEmulator() && currentState) {
                return this.go(currentState);
            }
            return this.go("main.home");
        }

    }

    routeService["$inject"] = ["$state", "contextService", "settingsService", "localStorageService", "loadingService", "$rootScope", "routeConstants"];

    mobileServices.service('routeService', routeService);


})(mobileServices)
