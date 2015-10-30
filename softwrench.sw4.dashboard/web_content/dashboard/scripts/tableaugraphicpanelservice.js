(function (angular, tableau) {
    "use strict";

    function tableauGraphicPanelService($q, restService, contextService) {
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
            contextService.set("avoidspin", true, true);
            config.authPromise = restService
                .postPromise("Dashboard", "Authenticate", { provider: provider || config.defaultProvider })
                .then(function (response) {
                    config.auth = response.data["resultObject"];
                    return config.auth;
                })
                .catch(function(err) {
                    config.authPromise = null;
                    return $q.reject(err);
                })
                .finally(function() {
                    contextService.set("avoidspin", false, true);
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

        function resizeGraphic(graphic, width, height) {
            return graphic.setFrameSize(width, height);
        }

        /**
         * Authenticates to tableau server (if not already authenticated) 
         * and populates the associationOptions with the user's workbooks.
         * 
         * @param {} event 
         */
        function onProviderSelected(event) {
            if (!event.scope.associationOptions) event.scope.associationOptions = {};
            authenticate(event.fields.provider)
                .then(function (auth) {
                    // mocking call to rest api
                    return [
                        { id: "2", name: "Regional" },
                        { id: "3", name: "Superstore" }
                    ];
                })
                .then(function (workbooks) {
                    if (!event.scope.associationOptions) event.scope.associationOptions = {};
                    event.scope.associationOptions.workbooks = workbooks.map(function(w) {
                        w.value = {id: w.id, name: w.name}
                        w.label = w.name;
                        return w;
                    });
                });
        }

        function onWorkbookSelected(event) {
            if (!event.fields.workbook) return;
            // mocking call to rest api
            var regional = [{ name: "Obesity" }, { name: "Storms" }];
            var superstore = [{ name: "Overview" }, { name: "Product" }];
            var views = event.fields.workbook.id === "2" ? regional : superstore;
            $q.when(views).then(function(result) {
                if (!event.scope.associationOptions) event.scope.associationOptions = {};
                event.scope.associationOptions.views = result.map(function (v) {
                    v.value = {name: v.name}
                    v.label = v.name;
                    return v;
                });
            });
        }

        function onBeforeAssociatePanel(datamap) {
            datamap.configurationDictionary = {
                'workbook': datamap.workbook.name,
                'view': datamap.view.name
            };
            //datamap.configuration = Object.keys(datamap.configurationDictionary).map(function(key) {
            //    return key + "=" + datamap.configurationDictionary[key];
            //}).join(";");
            delete datamap["workbook"];
            delete datamap["view"];
        }

        //#endregion

        //#region Service Instance
        var service = {
            authenticate: authenticate,
            renderGraphic: renderGraphic,
            loadGraphic: loadGraphic,
            isAuthenticated: isAuthenticated,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onWorkbookSelected: onWorkbookSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("tableauGraphicPanelService", ["$q", "restService", "contextService", tableauGraphicPanelService]);
    //#endregion

})(angular, tableau);
