(function (angular) {
    "use strict";

    let areSettingsFullFilled;

    class initialRouterService {

        constructor($log,$q, settingsService, securityService, loadingService, swAlertPopup, routeService,crudContextService) {
            this.$log = $log;
            this.$q = $q;
            this.settingsService = settingsService;
            this.securityService = securityService;
            this.loadingService = loadingService;
            this.swAlertPopup = swAlertPopup;
            this.routeService = routeService;
            this.crudContextService = crudContextService;

            areSettingsFullFilled = function (settings) {
                return !!settings && !!settings.serverurl && settings.serverurl !== "http://";
            }
        }

        doInit() {

            const log = this.$log.get("initialRouteService#doInit", ["init", "bootstrap"]);

            const authenticated = this.securityService.hasAuthenticatedUser();
            this.crudContextService.restoreState();

            //should not be present at production mode
            const localdata = window.localdevdata;
            if (localdata && !!localdata.debuglogs) {
                const debugarr = localdata.debuglogs;
                debugarr.forEach(log => {
                    swlog.debug(log);
                });
            }

            if (!localdata || localdata.showlogin) {
                log.debug("redirecting to initial state");
                return this.routeService.loadInitialState(authenticated);
            }

            

            log.debug("applying local rules");

            //if we are on development mode let´s handle authentication automatically to boost productivity
            return this.settingsService.getSettings().then(s => {
                
                

                if (areSettingsFullFilled(s) && authenticated) {
                    this.crudContextService.restoreState();
                    return this.routeService.loadInitialState(authenticated);
                }

                this.loadingService.showDefault();
                const serverurl = isRippleEmulator() ? localdata.serverurl.ripple : localdata.serverurl.device;

                const that = this;

                return this.settingsService.saveSettings({ serverurl })
                    .then(() => {
                        return that.securityService.login(localdata.username, localdata.password);
                    }).then(() => {
                        return that.routeService.go("main.home");
                    }).catch(function(error) {
                        that.securityService.logout();
                        that.swAlertPopup.show({
                            title: "Login failed",
                            template: !!error && !!error.message ? error.message : "Please check your credentials."
                        });
                        return that.routeService.loadInitialState(authenticated);
                    }).finally(function() {
                        that.loadingService.hide();
                    });
            });

            

            
        }
    }


    initialRouterService["$inject"] = ["$log", "$q", "settingsService", "securityService", "loadingService", "swAlertPopup", "routeService", "crudContextService"];

    angular.module("sw_mobile_services").service("initialRouterService", initialRouterService);

})(angular);