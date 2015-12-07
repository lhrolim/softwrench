(function (angular, tableau, $) {
    "use strict";

    function tableauGraphicPanelService($q, restService, crudContextHolderService, localStorageService, contextService) {
        //#region Utils
        var config = {
            defaultProvider: "Tableau",
            auth: {
                ticketTTL: 3 * 60 * 1000, // 3min trusted ticket timeout
                restTTL: 4 * 60 * 60 * 1000, // 240min rest token timeout
                cacheRegion: {}
            },
            render: {
                options: {
                    hideToolbar: true,
                    hideTabs: true
                }
            }
        }

        function sanitizeName(name) {
            return window.replaceAll(name, " ", "");
        }

        function viewUrl(panel, auth) {
            var workbook = sanitizeName(panel.configurationDictionary["workbook"]);
            var view = sanitizeName(panel.configurationDictionary["view"]);
            var ticket = auth.ticket;
            var site = auth.siteName;
            var serverUrl = auth.serverUrl.endsWith("/") ? auth.serverUrl.substring(0, auth.serverUrl.length - 1) : auth.serverUrl;
            var url = "{0}{1}{2}/views/{3}/{4}".format(serverUrl, (!!ticket ? "/trusted/" + ticket : ""), (!!site ? "/site/" + site : ""), workbook, view);
            return url;
        }

        function buildRenderOptions(element, options) {
            var built = angular.copy(config.render.options);
            built.height = element.parentElement.offsetHeight;
            built.width = element.parentElement.offsetWidth;
            if (!options) return built;
            angular.forEach(options, function (value, key) {
                built[key] = value;
            });
            return built;
        }

        function toObjectList(xml, tagName) {
            var parsed = $.parseXML(xml);
            var elements = parsed.getElementsByTagName(tagName);
            // slice: hack to turn an HTMLCollection into an Array
            return Array.prototype.slice.call(elements).map(function (element) {
                var attrs = element.attributes;
                var object = {};
                angular.forEach(attrs, function (attr) {
                    object[attr.name] = attr.value;
                });
                return object;
            });
        }

        function trustedTicketCacheKey(panel) {
            // cache key composed of user, workbook and view requested:
            // tableau expects a different trusted ticket for each different view
            var workbook = sanitizeName(panel.configurationDictionary["workbook"]);
            var view = sanitizeName(panel.configurationDictionary["view"]);
            var user = contextService.getUserData().login;
            return "sw:graphic:tableau:auth:ticket:" + user + ":" + workbook + ":" + view;
        }
        
        /**
         * Authenticates the user to the Tableau in one of two manners: 
         * - authenticates to REST api (retrieves token and user info)
         * - fetches trusted ticket for the Javascript api
         * 
         * @param String type authentication type: ["REST" or "TICKET"] 
         * @param String cacheKey localstorage key to store the authentication response 
         * @param Long cachettl authentication response's cache TTL (in milliseconds)
         * @returns Promise resolved with the authentication response 
         */
        function authenticate(type, cacheKey, cachettl) {
            // hit cache first
            var cachedAuth = localStorageService.get(cacheKey);
            if (!!cachedAuth) return $q.when(cachedAuth);
            
            // hit in-progress request promise (avoid multiple identical requests)
            var undergoingPromiseCacheKey = cacheKey + ":promise";
            var undergoingPromise = config.auth.cacheRegion[undergoingPromiseCacheKey];
            if (!!undergoingPromise) return undergoingPromise;            
            
            // execute request
            var params = { provider: config.defaultProvider };
            var payload = { authtype: type };
            var ajaxconfig = { avoidspin: true };
            var authPromise = restService.postPromise("Dashboard", "Authenticate", params, payload, ajaxconfig)
                .then(function (response) {
                    var auth = response.data["resultObject"];
                    // update cache
                    localStorageService.put(cacheKey, auth, { ttl: cachettl });
                    return auth;
                })
                .finally(function () {
                    // clear in-progress promise
                    config.auth.cacheRegion[undergoingPromiseCacheKey] = null;
                });

            // cache in-progress promise
            config.auth.cacheRegion[undergoingPromiseCacheKey] = authPromise;

            return authPromise;
        }

        function fetchTrustedTicket(panel) {
            return authenticate("TICKET", trustedTicketCacheKey(panel), config.auth.ticketTTL);
        }

        function authToRestApi() {
            var cacheKey = "sw:graphic:tableau:auth:rest:" + contextService.getUserData().login;
            return authenticate("REST", cacheKey, config.auth.restTTL);
        }

        //#endregion

        //#region Public methods
       

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
            renderOptions.onFirstInteractive = function (event) {
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
            return fetchTrustedTicket(panel).then(function (auth) {
                return renderGraphic(element, panel, auth, options);
            });
        }

        function resizeGraphic(graphic, width, height) {
            return graphic.setFrameSize(width, height);
        }

        /**
         * Authenticates to tableau server's REST api (if not already authenticated) 
         * and populates the associationOptions with the user's workbooks.
         * 
         * @param {} event 
         */
        function onProviderSelected(event) {

            authToRestApi()
                .then(function (auth) {
                    var params = { provider: event.fields.provider, resource: "workbook" };
                    var payload = angular.copy(auth);
                    var requestconfig = { avoidspin: true };
                    return restService.postPromise("Dashboard", "LoadGraphicResource", params, payload, requestconfig);
                })
                .then(function (response) {
                    var workbooks = toObjectList(response.data, "workbook").map(function (workbook) {
                        workbook.value = { id: workbook.id, name: workbook.name }
                        workbook.label = workbook.name;
                        return workbook;
                    });
                    crudContextHolderService.updateEagerAssociationOptions("workbooks", workbooks);
                });
        }

        /**
         * Authenticates to tableau server's REST api (if not already authenticated) 
         * and populates the associationOptions with the selected workbook's views.
         * 
         * @param {} event 
         */
        function onWorkbookSelected(event) {
            if (!event.fields.workbook) return;

            authToRestApi()
                .then(function (auth) {
                    var params = { provider: event.fields.provider, resource: "view" };
                    var payload = angular.copy(auth);
                    payload.workbook = event.fields.workbook.id;
                    var requestconfig = { avoidspin: true };
                    return restService.postPromise("Dashboard", "LoadGraphicResource", params, payload, requestconfig);
                })
                .then(function (response) {
                    var views = toObjectList(response.data, "view").map(function (view) {
                        view.value = { name: view.name }
                        view.label = view.name;
                        return view;
                    });
                    crudContextHolderService.updateEagerAssociationOptions("views", views);
                });
        }

        /**
         * Fills the datamps's 'configurationDictionary' property correctly 
         * from it's view properties.
         * 
         * @param {} datamap 
         */
        function onBeforeAssociatePanel(datamap) {
            datamap.configurationDictionary = {
                'workbook': datamap.workbook.name,
                'view': datamap.view.name
            };
        }

        //#endregion

        //#region Service Instance
        var service = {
            renderGraphic: renderGraphic,
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onWorkbookSelected: onWorkbookSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular
        .module("sw_layout")
        .factory("tableauGraphicPanelService",
            ["$q", "restService", "crudContextHolderService", "localStorageService", "contextService", tableauGraphicPanelService]);
    //#endregion

})(angular, tableau, jQuery);
