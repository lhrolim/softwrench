
(function (angular) {
    "use strict";

    function historyService($rootScope, $location, $log, $timeout, $q, crudContextHolderService, i18NService, contextService, alertService, localStorageService, restService) {
        //#region Utils
        var breadcrumbHistoryKey = "breadcrumbHistory";
        var cancelReturnKey = "cancelReturn";
        const routeInfoKey = `${url("")}:routeInfo`;

        const applicationDecorationObject = {
            "person": "user",
            "servicerequest": "sr",
            "quickservicerequest": "quicksr",
            "workorder": "wo",
            "_UserProfile": "securitygroup",
            "_SoftwrenchError": "error"
        }

        const customPathObject = {
            "/api/generic/Configuration/About": "/web/about",
            "/api/generic/DashBoard": "/web/dashboard"
        }

        const myProfileBaseUrl = "/api/data/person?userid=";

        function buildTitleFromSchema() {
            return i18NService.getI18nRecordLabel(crudContextHolderService.currentSchema(), crudContextHolderService.rootDataMap());
        }

        function createBreadCrumbEntry(url, addCurrentLabel) {
            const entry = {};
            if (addCurrentLabel) {
                entry.title = buildTitleFromSchema();
            }
            entry.url = url;
            return entry;
        }

        // add some info from schema to the first entry to be able to recreate application node on breadcrumb
        function addSchemaInfo(breadcrumbEntry) {
            const schema = crudContextHolderService.currentSchema();
            if (!schema) {
                return;
            }

            breadcrumbEntry.applicationName = schema.applicationName;
            breadcrumbEntry.schemaTitle = schema.title;
        }

        function addToBreadcrumbHistory(url) {
            var breadcrumbHistory = getBreadcrumbHistory();
            var currentUrl = getLocationUrl();

            // adds the title for the last entry (possibly schema info for the first entry)
            if (breadcrumbHistory.length === 0 && currentUrl) {
                const firstEntry = createBreadCrumbEntry(currentUrl, true);
                addSchemaInfo(firstEntry);
                breadcrumbHistory.push(firstEntry);
            } else if (currentUrl) {
                breadcrumbHistory.forEach(function (entry) {
                    if (entry.url === currentUrl) {
                        entry.title = buildTitleFromSchema();
                    }
                });
            }

            const state = getLocationState();
            const indexFromLocation = state && state.BcHistoryIndex != null ? state.BcHistoryIndex : -1;
            if (indexFromLocation >= 0 && indexFromLocation < breadcrumbHistory.length - 1) {
                breadcrumbHistory = breadcrumbHistory.slice(0, indexFromLocation + 1);
            }

            breadcrumbHistory.push(createBreadCrumbEntry(url, false));
            contextService.set(breadcrumbHistoryKey, breadcrumbHistory);
            return breadcrumbHistory.length - 1;
        }

        // workaround - a way to know if the location was changed by adding a url to the history (causes change in hash)
        // or by browser back and forward
        var locationUpdatedByService = false;

        function getHashBase64() {
            const hash = $location.hash();
            if (hash && hash.startsWith("state=")) {
                return hash.substring(6);
            }

            return null;
        }

        function customPath(stateUrl, contextPath) {
            if (stateUrl == null) {
                return null;
            }

            const user = contextService.getUserData();
            const username = user.login;
            const myprofilePath = contextPath + myProfileBaseUrl + username;
            if (stateUrl.toLowerCase().startsWith(myprofilePath)) {
                return `${contextPath}/web/myprofile`;
            }

            var foundPath = null;
            angular.forEach(customPathObject, (path, prefix) => {
                if (stateUrl.startsWith(contextPath + prefix)) {
                    foundPath = path;
                }
            });

            // ReSharper disable once ConditionIsAlwaysConst
            // ReSharper disable once HeuristicallyUnreachableCode
            if (foundPath) {
                return contextPath + foundPath;
            }
            return null;
        }

        function changePath(path, siteId) {
            $location.path(path);
            if (siteId) {
                $location.search("siteid", siteId);
            } else {
                $location.search({});
            }
            const hash = $location.hash();
            if (!!hash && hash.startsWith("tabid")) {
                contextService.setActiveTab(hash.substring("tabid=".length));
            }

            
        }

        function doUpdatePath(stateUrl, routeInfo) {
            if (!routeInfo || !routeInfo.contextPath) {
                return null;
            }

            const contextPath = routeInfo.contextPath;

            const foundCustomPath = customPath(stateUrl, contextPath);
            if (foundCustomPath) {
                return changePath(foundCustomPath);
            }

            const dataPrefix = contextPath + "/api/data/";

            const dataPrefixUpper = contextPath + "/api/Data/";
            const isPrefixed = stateUrl.startsWith(dataPrefix);
            const isPrefixedUpper = stateUrl.startsWith(dataPrefixUpper);
            if (!isPrefixed && !isPrefixedUpper) {
                if (stateUrl.contains("/api")) {
                    return changePath(contextPath);   
                }

                if (!stateUrl.startsWith("/")) {
                    stateUrl = "/" + stateUrl;
                }

                return changePath(`${contextPath}${stateUrl}`);
            }

            const realPrefix = isPrefixed ? dataPrefix : dataPrefixUpper;

            const decoded = decodeURI(stateUrl);
            const tokens = decoded.split("?");
            const path = tokens[0];
            const paramsStr = tokens[1];
            if (!paramsStr) {
                return changePath(contextPath);
            }

            let application = path.substring(realPrefix.length).split("/")[0];
            if (!application) {
                return changePath(contextPath);
            }
            application = application.toLowerCase();

            const params = JSON.parse(`{"${paramsStr.replace(/"/g, '\\"').replace(/&/g, '","').replace(/=/g, '":"')}"}`);

            if (!!params.aliasurl) {
                if (!params.aliasurl.startsWith("/")) {
                    params.aliasurl = "/" + params.aliasurl;
                }

                return changePath(`${contextPath }${params.aliasurl}`);
            }

            const schemaId = params["key[schemaId]"] || params["key[schemaid]"];
            if (!schemaId) {
                return changePath(contextPath);
            }

            if (!routeInfo.schemaInfo || !routeInfo.schemaInfo[application]) {
                return changePath(contextPath);
            }

            const schemaInfo = routeInfo.schemaInfo[application];

            const decoratedApplication = applicationDecorationObject[application] || application;

            const listSchema = schemaInfo["listSchema"];
            if ((listSchema && listSchema === schemaId) || "list".equalsIc(schemaId)) {
                return changePath(`${contextPath}/web/${decoratedApplication}`);
            }

            const id = params["id"];

            const newDetailSchema = schemaInfo["newDetailSchema"];
            if (((newDetailSchema && newDetailSchema === schemaId) || "detailnew".equalsIc(schemaId)) && !id) {
                return changePath(`${contextPath}/web/${decoratedApplication}/new`);
            }

            const detailSchema = schemaInfo["detailSchema"];
            if ((!detailSchema || detailSchema !== schemaId) && !"detail".equalsIc(schemaId)) {
                return changePath(contextPath);
            }

            if (id) {
                return changePath(`${contextPath}/web/${decoratedApplication}/uid/${id}`);
            }

            const userId = params["userId"];
            const siteId = params["siteId"];
            if (userId) {
                return changePath(`${contextPath}/web/${decoratedApplication}/${userId}`, siteId);
            }

            return changePath(contextPath);
        }

        function updatePath(stateUrl) {
            if (stateUrl) {
                getRouteInfo().then(routeInfo => doUpdatePath(stateUrl, routeInfo));    
            }
        }

        function updateState(state) {
            const hash = `state=${Base64.encode(JSON.stringify(state))}`;
//            $location.hash(hash);
        }

        function getLocationState() {
            const log = $log.getInstance("historyService#getLocationState");

            const base64 = getHashBase64();
            if (!base64) {
                log.debug("No history state on current location.");
                return null;
            }

            var state;
            try {
                state = JSON.parse(Base64.decode(base64));
            } catch (e) {
                log.debug("It was not possible to parse the state from the current location:\n{0}".format(e));
                return null;
            }

            return state;
        }

        function innerRedirect(url, historyIndex, currentIndex) {
            // sets history index in case of the same url appears more than once on history
            const state = {
                url: url,
                BcHistoryIndex: historyIndex
            }
            updateState(state);
            updatePath(state.url);

            // fires an event if the src and target have the same title
            // to force the breadcrumb update the current page
            if (currentIndex < 0 || historyIndex < 0) {
                return;
            }
            const history = getBreadcrumbHistory();
            const current = history[currentIndex];
            const target = history[historyIndex];
            if (!current || !target) {
                return;
            }
            if (current.title === target.title) {
                $rootScope.$broadcast("sw.breadcrumb.history.redirect.sametitle");
            }
        }

        function redirect(url, historyIndex, currentIndex, msg) {
            if (!crudContextHolderService.getDirty()) {
                innerRedirect(url, historyIndex, currentIndex);
                return;
            }

            const confirmMsg = msg || "Are you sure you want to leave the page?";

            alertService.confirmCancel(confirmMsg).then(function () {
                // alternative $scope.digest
                $timeout(function () {
                    crudContextHolderService.clearDirty();
                    crudContextHolderService.clearDetailDataResolved();
                    innerRedirect(url, historyIndex, currentIndex);
                }, 0);
            }, function () { return; });
        }

        function doSaveCancelReturn() {
            const state = getLocationState();
            contextService.set(cancelReturnKey, state);
        }
        //#endregion

        //#region Public methods for history

        function addToHistory(url, saveHistoryReturn, saveCancelReturn) {
            const state = { url: url };
            if (saveHistoryReturn) {
                state.BcHistoryIndex = addToBreadcrumbHistory(url);
            } else {
                state.BcHistoryIndex = 0;
            }

            // save the return for cancel button
            // for breadcrumb navigation breadcrumb history is used
            if (saveCancelReturn) {
                doSaveCancelReturn();
            }

            const log = $log.getInstance("historyService#addToHistory");
            log.debug("The url ({0}) is added to history.".format(url));

            locationUpdatedByService = true;

            if ($location.hash() === "") {
                $location.replace();
            }
            updateState(state);
            updatePath(state.url);
        }

        function getLocationUrl() {
            const log = $log.getInstance("historyService#getLocationUrl");

            const state = getLocationState();
            if (state == null) {
                return null;
            }

            const locationUrl = state.url;
            log.debug("The history url ({0}) was recovered from current location.".format(locationUrl));
            return locationUrl;
        }

        function wasLocationUpdatedByService() {
            return locationUpdatedByService;
        }

        function resetLocationUpdatedByService() {
            locationUpdatedByService = false;
        }

        function redirectOneBackHard() {
            window.history.back();
        }

        function redirectOneBack(msg) {
            const currentIndex = indexOnBreadcrumbHistory();

            // not on breadcrumb navigation or the first one in history
            // tries to redirect from cancel button return on session
            if (currentIndex <= 0) {
                const returnState = contextService.get(cancelReturnKey, true);
                if (!returnState) {
                    return false;
                }
                redirect(returnState.url, -1, -1, msg);
                return true;
            }

            const redirectIndex = currentIndex - 1;
            const breadcrumbHistory = getBreadcrumbHistory();
            const historyLength = breadcrumbHistory.length;
            if (redirectIndex >= historyLength) {
                return false;
            }

            const redirectEntry = breadcrumbHistory[redirectIndex];
            breadcrumbRedirect(redirectEntry.url, redirectIndex, msg);
            return true;
        }

        function getRouteInfo() {
            if (contextService.get("anonymous", false, true)) {
                return null;
            }

            var routeInfo = localStorageService.get(routeInfoKey);
            if (routeInfo) {
                return $q.when(routeInfo);
            }

            return restService.getPromise("Metadata", "GetRouteInfo").then(response => {
                routeInfo = response.data;
                localStorageService.put(routeInfoKey, routeInfo);
                return routeInfo;
            });
        }
        //#endregion

        //#region Public methods for breadcrumb history

        // gets the indexes of the current location on breadcrumb history
        function indexOnBreadcrumbHistory() {
            var indexOnHistory = -1;
            var url = getLocationUrl();
            if (!url) {
                return indexOnHistory;
            }

            // tries to find the current url on breadcrumb history
            // considers index from location state for the case that the same url appears more than once
            const state = getLocationState();
            var indexFromLocation = state && state.BcHistoryIndex != null ? state.BcHistoryIndex : -1;
            var breadcrumbHistory = getBreadcrumbHistory();
            breadcrumbHistory.reverse().every(function (entry, index) {
                if (entry.url !== url) {
                    return true;
                }

                // a reverse was done
                const possibleIndex = breadcrumbHistory.length - 1 - index;

                //indexOnHistory = possibleIndex;
                // no index on current location, picks the first found (last on array due to reverse)
                // or exacly same from current location
                if (indexFromLocation < 0 || possibleIndex === indexFromLocation) {
                    indexOnHistory = possibleIndex;
                    return false;
                }

                return true;
            });

            return indexOnHistory;
        }

        function eraseBreadcrumbHistory() {
            contextService.set(breadcrumbHistoryKey, []);
        }

        function getBreadcrumbHistory() {
            var breadcrumbHistory = contextService.get(breadcrumbHistoryKey, true);
            if (!breadcrumbHistory) {
                breadcrumbHistory = [];
                eraseBreadcrumbHistory();
            }
            return breadcrumbHistory;
        }

        // updates the title of the last item of the breadcrumb history
        // also updates the current history index
        function updateBreadcrumbHistoryTitle(title) {
            const history = getBreadcrumbHistory();
            history[history.length - 1].title = title;
            contextService.set(breadcrumbHistoryKey, history);
        }

        function breadcrumbRedirect(url, historyIndex, msg) {
            const currentIndex = indexOnBreadcrumbHistory();
            if (historyIndex === currentIndex) {
                return;
            }
            redirect(url, historyIndex, currentIndex, msg);
        }
        //#endregion

        //#region Service Instance
        const service = {
            // history methods
            addToHistory,
            getLocationUrl,
            wasLocationUpdatedByService,
            resetLocationUpdatedByService,
            redirectOneBack,
            redirectOneBackHard,
            getRouteInfo,
            // breadcrumb history methods
            indexOnBreadcrumbHistory,
            eraseBreadcrumbHistory,
            getBreadcrumbHistory,
            updateBreadcrumbHistoryTitle,
            breadcrumbRedirect,
            routeInfoKey
        };
        return service;
        //#endregion
    }

    //#region Service registration

    modules.webcommons.service("historyService", ["$rootScope", "$location", "$log", "$timeout", "$q", "crudContextHolderService", "i18NService", "contextService", "alertService", "localStorageService", "restService", historyService]);

    //#endregion

})(angular);