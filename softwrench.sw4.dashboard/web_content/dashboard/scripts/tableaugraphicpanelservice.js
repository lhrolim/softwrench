(function (angular, tableau) {
    "use strict";

    function tableauGraphicPanelService($q, restService) {
        //#region Utils
        var config = {
            auth: null,
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
                .postPromise("Dashboard", "Authenticate", { provider: provider || config.defaultProvider })
                .then(function (response) {
                    config.auth = response.data["resultObject"];
                    return config.auth;
                })
                .catch(function(err) {
                    config.authPromise = null;
                    return $q.reject(err);
                });
            return config.authPromise;
        }

        function viewUrl(panel, auth) {
            var workbook = panel.configurationDictionary["workbook"];
            var view = panel.configurationDictionary["view"];
            var ticket = auth.ticket;
            var site = auth.siteName; 
            var url = "{0}{1}{2}/views/{3}/{4}".format(auth.serverUrl, (!!ticket ? "/trusted/" + ticket : ""), (!!site ? "/site/" + site : ""), workbook, view);
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

        function isAuthenticated() {
            return !!config.auth;
        }

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

        /**
         * Authenticates user (if not already authenticated) then renders graphic.
         * 
         * @param DOMNode element 
         * @param {} panel 
         * @param {} options 
         * @returns Promise resolved with tableau Viz instantiated 
         */
        function loadGraphic(element, panel, options) {
            return authenticate().then(function(auth) {
                return renderGraphic(element, panel, auth, options);
            });
        }

        //#endregion

        //#region Service Instance
        var service = {
            authenticate: authenticate,
            renderGraphic: renderGraphic,
            loadGraphic: loadGraphic,
            isAuthenticated: isAuthenticated
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("tableauGraphicPanelService", ["$q", "restService", tableauGraphicPanelService]);
    //#endregion

})(angular, tableau);
