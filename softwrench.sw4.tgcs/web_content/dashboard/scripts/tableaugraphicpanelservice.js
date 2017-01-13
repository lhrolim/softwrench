(function (angular, tableau, $) {
    "use strict";

    function tableauGraphicPanelService($q, restService, crudContextHolderService, localStorageService, contextService) {
        //#region Utils
        var config = {
            defaultProvider: "Tableau",
            auth: {
                ticketTTL: null, // don't cache ticket values, they're only worth a single view at a time 
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
            const workbook = sanitizeName(panel.configurationDictionary["workbook"]);
            const view = sanitizeName(panel.configurationDictionary["view"]);
            const ticket = auth.ticket;
            const site = auth.siteName;
            const serverUrl = auth.serverUrl.endsWith("/") ? auth.serverUrl.substring(0, auth.serverUrl.length - 1) : auth.serverUrl;
            const url = "{0}{1}{2}/views/{3}/{4}".format(serverUrl, (!!ticket ? "/trusted/" + ticket : ""), (!!site ? "/site/" + site : ""), workbook, view);
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
            const parsed = $.parseXML(xml);
            const elements = parsed.getElementsByTagName(tagName);
            // slice: hack to turn an HTMLCollection into an Array
            return Array.prototype.slice.call(elements).map(function (element) {
                const attrs = element.attributes;
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
            const workbook = sanitizeName(panel.configurationDictionary["workbook"]);
            const view = sanitizeName(panel.configurationDictionary["view"]);
            const user = contextService.getUserData().login;
            return "sw:graphic:tableau:auth:ticket:" + user + ":" + workbook + ":" + view;
        }
        
        /**
         * Authenticates the user to the Tableau in one of two manners: 
         * - authenticates to REST api (retrieves token and user info)
         * - fetches trusted ticket for the Javascript api
         * Caches the response with the appropriate timeout. 
         * Caches the promises to avoid multiple simultaneous identical requests.
         * 
         * @param String type authentication type: ["REST" or "TICKET"] 
         * @param String cacheKey localstorage key to store the authentication response 
         * @param Long cachettl authentication response's cache TTL (in milliseconds): if negative or NaN the response won't be cached
         * @returns Promise resolved with the authentication response 
         */
        function authenticate(type, cacheKey, cachettl) {
            var shouldCache = angular.isNumber(cachettl) && cachettl > 0;
            // hit cache first
            if (shouldCache) {
                const cachedAuth = localStorageService.get(cacheKey);
                if (!!cachedAuth) return $q.when(cachedAuth);
            }
            // hit in-progress request promise (avoid multiple identical requests)
            var undergoingPromiseCacheKey = cacheKey + ":promise";
            const undergoingPromise = config.auth.cacheRegion[undergoingPromiseCacheKey];
            if (!!undergoingPromise) return undergoingPromise;            
            
            // execute request
            const params = { provider: config.defaultProvider };
            const payload = { authtype: type };
            const ajaxconfig = { avoidspin: true };
            const authPromise = restService.postPromise("Dashboard", "Authenticate", params, payload, ajaxconfig)
                .then(function (response) {
                    const auth = response.data["resultObject"];
                    // update cache
                    if(shouldCache) localStorageService.put(cacheKey, auth, { ttl: cachettl });
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
            const cacheKey = "sw:graphic:tableau:auth:rest:" + contextService.getUserData().login;
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
            const url = viewUrl(panel, auth);
            const renderOptions = buildRenderOptions(element, options);
            var deferred = $q.defer();
            renderOptions.onFirstInteractive = function (event) {
                deferred.resolve(event.getViz());
            };
            try {
                const viz = new tableau.Viz(element, url, renderOptions);
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
                    const params = { provider: event.fields.provider, resource: "workbook" };
                    const payload = angular.copy(auth);
                    const requestconfig = { avoidspin: true };
                    return restService.postPromise("Dashboard", "LoadGraphicResource", params, payload, requestconfig);
                })
                .then(function (response) {
                    const workbooks = toObjectList(response.data, "workbook").map(function (workbook) {
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
        //afterchange
        function onWorkbookSelected(event) {
            if (!event.fields.workbook) return;

            authToRestApi()
                .then(function (auth) {
                    const params = { provider: event.fields.provider, resource: "view" };
                    const payload = angular.copy(auth);
                    payload.workbook = event.fields.workbook.id;
                    const requestconfig = { avoidspin: true };
                    return restService.postPromise("Dashboard", "LoadGraphicResource", params, payload, requestconfig);
                })
                .then(function (response) {
                    const views = toObjectList(response.data, "view").map(function (view) {
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


        function onDashboardSelected(graphic) {
            // implementing interface        
        }

        //#endregion

        //#region Service Instance
        const service = {
            renderGraphic: renderGraphic,
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onWorkbookSelected: onWorkbookSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel,
            onDashboardSelected: onDashboardSelected
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular
        .module("sw_layout")
        .service("tableauGraphicPanelService",
            ["$q", "restService", "crudContextHolderService", "localStorageService", "contextService", tableauGraphicPanelService]);
    //#endregion

})(angular, tableau, jQuery);
