
(function (angular) {
    "use strict";

    function historyService($rootScope, $location, $log, $timeout, crudContextHolderService, i18NService, contextService, alertService) {
        //#region Utils
        var breadcrumbHistoryKey = "breadcrumbHistory";
        var cancelReturnKey = "cancelReturn";

        function buildTitleFromSchema() {
            return i18NService.getI18nRecordLabel(crudContextHolderService.currentSchema(), crudContextHolderService.rootDataMap());
        }

        function createBreadCrumbEntry(url, addCurrentLabel) {
            var entry = {};
            if (addCurrentLabel) {
                entry.title = buildTitleFromSchema();
            }
            entry.url = url;
            return entry;
        }

        // add some info from schema to the first entry to be able to recreate application node on breadcrumb
        function addSchemaInfo(breadcrumbEntry) {
            var schema = crudContextHolderService.currentSchema();
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
                var firstEntry = createBreadCrumbEntry(currentUrl, true);
                addSchemaInfo(firstEntry);
                breadcrumbHistory.push(firstEntry);
            } else if (currentUrl) {
                breadcrumbHistory.forEach(function (entry) {
                    if (entry.url === currentUrl) {
                        entry.title = buildTitleFromSchema();
                    }
                });
            }

            var state = getLocationState();
            var indexFromLocation = state && state.BcHistoryIndex != null ? state.BcHistoryIndex : -1;
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

        // firefox workaround - there  is some cases that firefox turns the hash into a path
        function isOnFirefoxPath() {
            if (BrowserDetect.browser.toLocaleLowerCase() !== "firefox") {
                return false;
            }
            var path = $location.path();
            return path && path.startsWith("/state=");
        }

        function getHashBase64() {
            // firefox workaround - there  is some cases that firefox turns the hash into a path
            if (isOnFirefoxPath()) {
                return $location.path().substring(7);
            }

            var hash = $location.hash();
            if (hash && hash.startsWith("state=")) {
                return hash.substring(6);
            }

            return null;
        }

        function updateState(state) {
            var hash = "state=" + Base64.encode(JSON.stringify(state));
            if (isOnFirefoxPath()) {
                $location.path("/" + hash);
            } else {
                $location.hash(hash);
            }
        }

        function getLocationState() {
            var log = $log.getInstance("historyService#getLocationState");

            var base64 = getHashBase64();
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
            var state = {
                url: url,
                BcHistoryIndex: historyIndex
            }
            updateState(state);

            // fires an event if the src and target have the same title
            // to force the breadcrumb update the current page
            if (currentIndex < 0 || historyIndex < 0) {
                return;
            }
            var history = getBreadcrumbHistory();
            var current = history[currentIndex];
            var target = history[historyIndex];
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

            var confirmMsg = msg || "Are you sure you want to leave the page?";
            alertService.confirmCancel(null, null, function () {
                // alternative $scope.digest
                $timeout(function () {
                    crudContextHolderService.clearDirty();
                    crudContextHolderService.clearDetailDataResolved();
                    innerRedirect(url, historyIndex, currentIndex);
                }, 0);
            }, confirmMsg, function () { return; });
        }

        function doSaveCancelReturn() {
            var state = getLocationState();
            contextService.set(cancelReturnKey, state);
        }
        //#endregion

        //#region Public methods for history

        function addToHistory(url, saveHistoryReturn, saveCancelReturn) {
            var state = { url: url };
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

            var log = $log.getInstance("historyService#addToHistory");
            log.debug("The url ({0}) is added to history.".format(url));

            locationUpdatedByService = true;

            if ($location.hash() === "") {
                $location.replace();
            }
            updateState(state);
        }

        function getLocationUrl() {
            var log = $log.getInstance("historyService#getLocationUrl");

            var state = getLocationState();
            if (state == null) {
                return null;
            }

            var locationUrl = state.url;
            log.debug("The history url ({0}) was recovered from current location.".format(locationUrl));
            return locationUrl;
        }

        function wasLocationUpdatedByService() {
            return locationUpdatedByService;
        }

        function resetLocationUpdatedByService() {
            locationUpdatedByService = false;
        }

        function redirectOneBack(msg) {
            var currentIndex = indexOnBreadcrumbHistory();

            // not on breadcrumb navigation or the first one in history
            // tries to redirect from cancel button return on session
            if (currentIndex <= 0) {
                var returnState = contextService.get(cancelReturnKey, true);
                if (!returnState) {
                    return false;
                }
                redirect(returnState.url, -1, -1, msg);
                return true;
            }

            var redirectIndex = currentIndex - 1;
            var breadcrumbHistory = getBreadcrumbHistory();
            var historyLength = breadcrumbHistory.length;
            if (redirectIndex >= historyLength) {
                return false;
            }

            var redirectEntry = breadcrumbHistory[redirectIndex];
            breadcrumbRedirect(redirectEntry.url, redirectIndex, msg);
            return true;
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
            var state = getLocationState();
            var indexFromLocation = state && state.BcHistoryIndex != null ? state.BcHistoryIndex : -1;
            var breadcrumbHistory = getBreadcrumbHistory();
            breadcrumbHistory.reverse().every(function (entry, index) {
                if (entry.url !== url) {
                    return true;
                }

                // a reverse was done
                var possibleIndex = breadcrumbHistory.length - 1 - index;

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
            var history = getBreadcrumbHistory();
            history[history.length - 1].title = title;
            contextService.set(breadcrumbHistoryKey, history);
        }

        function breadcrumbRedirect(url, historyIndex, msg) {
            var currentIndex = indexOnBreadcrumbHistory();
            if (historyIndex === currentIndex) {
                return;
            }
            redirect(url, historyIndex, currentIndex, msg);
        }
        //#endregion

        //#region Service Instance
        var service = {
            // history methods
            addToHistory: addToHistory,
            getLocationUrl: getLocationUrl,
            wasLocationUpdatedByService: wasLocationUpdatedByService,
            resetLocationUpdatedByService: resetLocationUpdatedByService,
            redirectOneBack: redirectOneBack,
            // breadcrumb history methods
            indexOnBreadcrumbHistory: indexOnBreadcrumbHistory,
            eraseBreadcrumbHistory: eraseBreadcrumbHistory,
            getBreadcrumbHistory: getBreadcrumbHistory,
            updateBreadcrumbHistoryTitle: updateBreadcrumbHistoryTitle,
            breadcrumbRedirect: breadcrumbRedirect
        };
        return service;
        //#endregion
    }

    //#region Service registration

    modules.webcommons.factory("historyService", ["$rootScope", "$location", "$log", "$timeout", "crudContextHolderService", "i18NService", "contextService", "alertService", historyService]);

    //#endregion

})(angular);