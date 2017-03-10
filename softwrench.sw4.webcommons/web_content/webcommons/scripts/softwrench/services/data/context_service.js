!(function (modules, angular) {
    "use strict";

    modules.rootCommons.service("contextService", ["$log", "$rootScope", function ($log, $rootScope) {

        return {
            //using sessionstorage instead of rootscope, as the later would be lost upon F5.
            //see SWWEB-239
            insertIntoContext: function (key, value, userootscope) {
                const urlContext = url("");
                if (userootscope) {
                    $rootScope[urlContext + ':ctx_' + key] = value;
                } else {
                    if (value != null && !isString(value)) {
                        value = JSON.stringify(value);
                    }
                    sessionStorage[urlContext + ':ctx_' + key] = value;
                }
                return value;
            },

            set: function (key, value, userootscope) {
                return this.insertIntoContext(key, value, userootscope);
            },

            get: function (key, isJson, userootscope) {
                return this.fetchFromContext(key, isJson, userootscope);
            },

            fetchFromContext: function (key, isJson, userootscope, removeentry) {
                //shortcut method
                const value = this.retrieveFromContext(key, userootscope, removeentry);
                if (value == "undefined") {
                    return undefined;
                }
                if (value != null && isJson == true && isString(value)) {
                    return JSON.parse(value);
                }
                return value;
            },

            //shortcut method
            getFromContext: function (key, isJson, userootscope) {
                return this.fetchFromContext(key, isJson, userootscope);
            },

            retrieveFromContext: function (key, userootscope, removeentry) {
                const urlContext = url("");
                if (userootscope) {
                    const object = $rootScope[urlContext + ':ctx_' + key];
                    if (removeentry) {
                        delete $rootScope[urlContext + ':ctx_' + key];
                    }
                    return object;
                }
                const sessionContextValue = sessionStorage[urlContext + ':ctx_' + key];
                if (removeentry) {
                    sessionStorage.removeItem([urlContext + ':ctx_' + key]);
                }
                if (sessionContextValue == "null") {
                    return null;
                }
                return sessionContextValue;
            },

            deleteFromContext: function (key) {
                const urlContext = url("");
                delete sessionStorage[urlContext + ":ctx_" + key];
                delete $rootScope[urlContext + ":ctx_" + key];
            },

            isLocal: function () {
                if (localStorage.mocknonlocal || sessionStorage.mocknonlocal) {
                    return false;
                }
                if (angular.mock) {
                    //unit tests should be considerered local too
                    return true;
                }
                const contextValue = this.retrieveFromContext("isLocal");
                return !!contextValue && contextValue === "true";
            },

            isDev: function () {
                //return this.retrieveFromContext('environment') == "dev";

                //return true if the environment begins with dev
                const environment = this.retrieveFromContext("environment");
                return !environment || environment.indexOf("dev") === 0;
            },

            client: function () {
                return this.retrieveFromContext("clientName");
            },

            isClient: function (name) {
                const clientName = this.client();
                if (name === clientName) {
                    return true;
                }
                if (angular.isArray(name)) {
                    if (jQuery.inArray(clientName, name)) {
                        return true;
                    }
                }
                if (!name) {
                    $log.getInstance('contextService#isClient').warn("asked for null client name")
                    return false;
                }
                return false;
            },
            getUserData: function () {
                if (angular.mock) {
                    //for unit tests let´s return a mocked user
                    return { login: "testuser" };
                }

                if ($rootScope.user != null) {
                    //caching
                    return $rootScope.user;
                }
                const userData = this.retrieveFromContext('user');
                if (userData == null) {
                    return null;
                }
                const user = JSON.parse(userData);
                $rootScope.user = user;
                return user;
            },

            InModule: function (moduleArray) {
                if (moduleArray == null) {
                    return false;
                }
                var result = false;
                var currModule = this.currentModule();
                if (nullOrUndef(currModule)) {
                    return false;
                }
                $.each(moduleArray, function (key, value) {
                    if (value.equalIc(currModule)) {
                        result = true;
                        return;
                    }
                });
                return result;

            },

            //determines whether the current user has one of the roles specified on the array
            HasRole: function (roleArray) {
                if (roleArray == null) {
                    return true;
                }
                const user = this.getUserData();
                var userroles = user.roles;
                var result = false;
                $.each(roleArray, function (key, value) {
                    $.each(userroles, function (k, v) {
                        if (v.name == value) {
                            result = true;
                            return;
                        }
                    });
                });
                return result;
            },

            loadUserContext: function (userData) {
                //clear cache
                $rootScope.user = userData;
                this.insertIntoContext('user', JSON.stringify(userData));
            },

            loadConfigs: function (config) {
                this.insertIntoContext('clientName', config.clientName);
                this.insertIntoContext('environment', config.environment);
                this.insertIntoContext('isLocal', config.isLocal);
                this.insertIntoContext('i18NRequired', config.i18NRequired);
                this.insertIntoContext('systeminittime', config.initTimeMillis);
                this.insertIntoContext('successMessageTimeOut', config.successMessageTimeOut);
                if (!config.clientSideLogLevel.equalsAny('warn', 'debug', 'info', 'error', 'none')) {
                    //to avoid the change of server side setting it to invalid
                    //TODO: config should allow list of options
                    config.clientSideLogLevel = 'warn';
                }
                this.insertIntoContext('defaultlevel', config.clientSideLogLevel.toLowerCase());
                this.insertIntoContext('invbalancesListScanOrder', config.invbalancesListScanOrder);
                // Add additional scan config keys here
                this.insertIntoContext('newInvIssueDetailScanOrder', config.newInvIssueDetailScanOrder);
                this.insertIntoContext('invIssueListScanOrder', config.invIssueListScanOrder);
                this.insertIntoContext('physicalcountListScanOrder', config.physicalcountListScanOrder);
                this.insertIntoContext('physicaldeviationListScanOrder', config.physicaldeviationListScanOrder);
                this.insertIntoContext('matrectransTransfersListScanOrder', config.matrectransTransfersListScanOrder);
                this.insertIntoContext('reservedMaterialsListScanOrder', config.reservedMaterialsListScanOrder);
                this.insertIntoContext('invIssueListBeringScanOrder', config.invIssueListBeringScanOrder);
                this.insertIntoContext('newKeyISsueDetailScanOrder', config.newKeyIssueDetailScanOrder);

                this.insertIntoContext("activityStreamFlag", config.activityStreamFlag, true);
                this.insertIntoContext("crudSearchFlag", config.crudSearchFlag, true);
                this.insertIntoContext("UIShowClassicAdminMenu", config.uiShowClassicAdminMenu, true);
                this.insertIntoContext("UIShowToolbarLabels", config.uiShowToolbarLabels, true);
                this.insertIntoContext("isLocal", config.isLocal);

                let dateTimeFormat = config.displayableFormats.dateTimeFormat;

                if (dateTimeFormat.indexOf("hh") !== -1 && dateTimeFormat.indexOf("a") === -1) {
                    dateTimeFormat += " a";
                }

                this.insertIntoContext('dateTimeFormat', dateTimeFormat);

                $rootScope.defaultEmail = config.defaultEmail;
                $rootScope.clientName = config.clientName;
                $rootScope.environment = config.environment;
                $rootScope.i18NRequired = config.i18NRequired;
            },

            getResourceUrl: function (path) {
                const baseURL = url(path);
                if (!this.isLocal()) {
                    const initTime = this.getFromContext("systeminittime");
                    if (baseURL.indexOf("?") == -1) {
                        return baseURL + "?" + initTime;
                    }
                    return baseURL + "&" + initTime;
                }
                return baseURL;
            },


            currentModule: function () {
                return this.retrieveFromContext('currentmodule');
            },

            clearContext: function () {
                const urlContext = url("");
                var i = sessionStorage.length;
                while (i--) {
                    const key = sessionStorage.key(i);
                    if (key.startsWith(urlContext + ':ctx_')) {
                        sessionStorage.removeItem(key);
                    }
                }
            },

            insertReportSearchDTO: function (reportSchemaId, searchDTO) {
                this.insertIntoContext('repSearchDTO_' + reportSchemaId, searchDTO);
            },

            retrieveReportSearchDTO: function (reportSchemaId) {
                return this.retrieveFromContext('repSearchDTO_' + reportSchemaId);
            },

            setActiveTab: function (tabId) {
                this.insertIntoContext('currenttab', tabId);
            },

            getActiveTab: function () {
                return this.fetchFromContext('currenttab');
            },
        }

    }]);

})(modules, angular);