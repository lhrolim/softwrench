(function (angular, tableau, $) {
    "use strict";

    function tableauGraphicPanelService($q, restService, crudContextHolderService) {
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
            var params = { provider: provider || config.defaultProvider };
            var requestconfig = { avoidspin: true };
            config.authPromise = restService
                .postPromise("Dashboard", "Authenticate", params, null, requestconfig)
                .then(function (response) {
                    config.auth = response.data["resultObject"];
                    return config.auth;
                })
                .catch(function (err) {
                    config.authPromise = null;
                    return $q.reject(err);
                });
            return config.authPromise;
        }

        function viewUrl(panel, auth) {
            var workbook = window.replaceAll(panel.configurationDictionary["workbook"], " ", "");
            var view = window.replaceAll(panel.configurationDictionary["view"], " ", "");
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
            return authenticate().then(function (auth) {
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
            authenticate(event.fields.provider)
                .then(function (auth) {
                    var params = { provider: event.fields.provider, resource: "workbook" };
                    var payload = { auth: auth };
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
         * Authenticates to tableau server (if not already authenticated) 
         * and populates the associationOptions with the selected workbook's views.
         * 
         * @param {} event 
         */
        function onWorkbookSelected(event) {
            if (!event.fields.workbook) return;
            authenticate(event.fields.provider)
                .then(function (auth) {
                    var params = { provider: event.fields.provider, resource: "view" };
                    var payload = { workbook: event.fields.workbook.id, auth: auth };
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
    angular.module("sw_layout").factory("tableauGraphicPanelService", ["$q", "restService", "crudContextHolderService", tableauGraphicPanelService]);
    //#endregion

})(angular, tableau, jQuery);
