(function (angular, tableau) {
    "use strict";

    function tableauGraphicPanelService($q, restService) {
        //#region Utils
        var config = {
            auth: { providerUrl: "http://10.50.100.171" }, //TODO: remove auth mock
            authPromise: null,
            defaultProvider: "Tableau",
            render: {
                options: {
                    hideToolbar: true,
                    hideTabs: true
                }
            }
        }

        function doAuthenticate(provider) {
            config.authPromise = restService
                .postPromise("Dashboard", "Auth", { provider: provider || config.defaultProvider })
                .then(function (response) {
                    config.auth = response.data["resultObject"];
                    return auth;
                });
            return config.authPromise;
        }

        function viewUrl(panel, auth) {
            var workbook = panel.configurationDictionary["workbook"];
            var view = panel.configurationDictionary["view"];
            var ticket = auth.ticket;
            var url = "{0}{1}/views/{2}/{3}".format(auth.providerUrl, (!!ticket ? "/trusted/" + ticket : ""), workbook, view);
            return url;
        }

        function buildRenderOptions(element, options) {
            var built = angular.copy(config.render.options);
            built.height = element.parentElement.offsetHeight;
            built.width = element.parentElement.offsetWidth;
            if (!options) return built;
            angular.forEach(options, function(value, key) {
                built[key] = value;
            });
            return built;
        }
        //#endregion

        //#region Public methods

        /**
         * Authenticates the user to the graphic provider server.
         * 
         * @param String provider 
         * @returns Promise resolved with the auth data 
         */
        function authenticate(provider) {
            // following technique from Dave Smith's presentation (https://www.youtube.com/watch?v=33kl0iQByME)
            // for handling multiple function calls avoiding sending multiple requests:
            return $q.when( // promise resolved with
                config.auth // cached result 
                || config.authPromise // already undergoing request's response
                || doAuthenticate(provider)); // new request's response
        }

        /**
         * Renders the requested graphic.
         * 
         * @param DOMNode element 
         * @param {} panel 
         * @param {} auth 
         * @param {} options 
         * @returns Promise resolved with the tableau Viz instantiated
         */
        function renderGraphic(element, panel, auth, options) {
            var url = viewUrl(panel, auth);
            var renderOptions = buildRenderOptions(element, options);
            var deferred = $q.defer();
            renderOptions.onFirstInteractive = function(event) {
                deferred.resolve(event.getViz());
            };
            try {
                var viz = new tableau.Viz(element, url, renderOptions);
            } catch (e) {
                deferred.reject(e);
            }
            return deferred.promise;
        }

        //#endregion

        //#region Service Instance
        var service = {
            authenticate: authenticate,
            renderGraphic: renderGraphic
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("tableauGraphicPanelService", ["$q", "restService", tableauGraphicPanelService]);
    //#endregion

})(angular, tableau);
