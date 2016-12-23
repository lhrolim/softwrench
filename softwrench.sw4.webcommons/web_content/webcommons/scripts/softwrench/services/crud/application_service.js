
(function (angular) {
    'use strict';



    angular.module('webcommons_services').service('applicationService', ["$q", '$http', '$rootScope','$injector', 'contextService', "crudContextHolderService", "userPreferencesService", "alertService", applicationService]);

    function fillApplicationParameters(parameters, applicationName, schemaId, mode) {
        /// <returns type=""></returns>
        if (parameters === undefined || parameters == null) {
            parameters = {};
        }
        parameters.applicationName = applicationName;
        parameters.key = {
            schemaId: schemaId,
            mode: mode,
            platform: platform()
        };
        parameters.currentSchemaKey = schemaId;
        return parameters;
    };

    function applicationService($q, $http, $rootScope,$injector, contextService, crudContextHolderService, userPreferencesService, alertService) {

        var buildApplicationURLForBrowser = function (applicationName, parameters) {
            var crudUrl = $(routes_homeurl)[0].value;
            const currentModule = contextService.retrieveFromContext('currentmodule');
            const currentMetadata = contextService.retrieveFromContext('currentmetadata');
            parameters.currentmodule = currentModule;
            parameters.currentmetadata = currentMetadata;
            var params = $.param(parameters);
            params = replaceAll(params, "=", "$");
            params = replaceAll(params, "&", "@");
            crudUrl = crudUrl + "?application=" + applicationName + "&popupmode=browser";
            if (!nullOrUndef(currentModule)) {
                crudUrl += "&currentModule=" + currentModule;
            }
            if (!nullOrUndef(currentMetadata)) {
                crudUrl += "&currentMetadata=" + currentMetadata;
            }
            crudUrl = crudUrl + "&querystring=" + params;
            return removeEncoding(crudUrl);
        };


        function toListSchema() {

        }


        function cancelDetail() {
            $('.no-touch [rel=tooltip]').tooltip('hide');

            if (!crudContextHolderService.getDirty()) {
                return toListSchema();
            }

            return alertService.confirmCancel().then(function () {
                toListSchema();
                crudContextHolderService.clearDirty();
                crudContextHolderService.clearDetailDataResolved();
            });
        }


        /**
         * 
         * @param {} parameters 
         * 
         * compositionData --> an object holding the composition related data:
         * {
         * 
         *       dispatcherComposition --> the name of the composition (relationship) that initiated this save
         *       operation --> indicates the operation performed on the composition, such as crud_delete, crud_update, crud_create
         *       id --> the id of the composition item
         * }
         * 
         * 
         * @returns {Promise} 
         */

        const defaultSaveParams = {
            refresh:false
        }

        function save({compositionData,dispatcherComposition,nextSchemaObj,dispatchedByModal,refresh, selecteditem, originalDatamap} = defaultSaveParams) {
            const isComposition = crudContextHolderService.getActiveTab() !== null;

            if (dispatchedByModal == undefined) {
                dispatchedByModal = crudContextHolderService.isShowingModal();
            }

            const panelId = dispatchedByModal ? "#modal" : null;

            const schema = crudContextHolderService.currentSchema(panelId);
            const datamap = crudContextHolderService.rootDataMap(panelId);

            //LUIZ -- TODO: review circular dependency
            return $injector.get('submitService').submit(schema, datamap, {
                isComposition,
                compositionData,
                dispatcherComposition,
                nextSchemaObj,
                dispatchedByModal,
                refresh,
                selecteditem,
                originalDatamap
            });

     
        }

        /// <summary>
        ///  refactor to use promises
        /// </summary>
        /// <param name="successCallBack"></param>
        /// <param name="failureCallback"></param>
        /// <param name="extraParameters"></param>
        function submitData(successCallBack, failureCallback, extraParameters) {

            extraParameters = extraParameters || {};
            const isComposition = extraParameters.isComposition;
            const nextSchemaObj = extraParameters.nextSchemaObj;
            const refresh = extraParameters.refresh;

            //TODO: refactor it entirely to use promises instead
            $rootScope.$broadcast(JavascriptEventConstants.CrudSubmitData, {
                successCbk: successCallBack,
                failureCbk: failureCallback,
                isComposition: isComposition,
                nextSchemaObj: nextSchemaObj,
                refresh: refresh

            });
        }

        function getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData) {
            /// <summary>
            ///  this method should be depracted, returning a promise instead
            /// </summary>
            /// <param name="applicationName"></param>
            /// <param name="schemaId"></param>
            /// <param name="mode"></param>
            /// <param name="title"></param>
            /// <param name="parameters">@deprecated --></param>
            /// <param name="jsonData"></param>
            parameters = fillApplicationParameters(parameters, applicationName, schemaId, mode);
            const pageSize = userPreferencesService.getSchemaPreference("pageSize", applicationName, schemaId);
            if (pageSize) {
                // searchdto added only because user pref - should mark to add preselected filters
                if (!parameters["SearchDTO"]) {
                    parameters["SearchDTO"] = { AddPreSelectedFilters: true }
                }
                parameters["SearchDTO"].pageSize = pageSize;
            }

            if (parameters.popupmode === "browser") {
                return buildApplicationURLForBrowser(applicationName, parameters);
            }
            if (title != null && title.trim() != "") {
                parameters.title = title;
            }
            if (jsonData == undefined) {
                //TODO:remove this
                return url("/api/data/" + applicationName + "?" + $.param(parameters));
            } else {
                parameters.application = applicationName;
                return url("/api/generic/ExtendedData/OpenDetailWithInitialData" + "?" + $.param(parameters));
            }
        };

        function getPostPromise(applicationName, schemaId, parameters, datamap) {
            parameters = fillApplicationParameters(parameters, applicationName, schemaId, mode);
            const postUrl = url("/api/data/" + applicationName);
            const jsonWrapper = {
                json: datamap,
                requestData: parameters
            };
            const jsonString = angular.toJson(jsonWrapper);
            return $http.post(postUrl, jsonString);
        };

        function invokeOperation(applicationName, schemaId, operation, datamap, extraParameters) {
            var parameters = extraParameters ? extraParameters : {};
            parameters.Operation = operation;

            parameters = fillApplicationParameters(parameters, applicationName, schemaId, null);
            const putUrl = url("/api/data/" + applicationName);
            const jsonWrapper = {
                json: datamap,
                requestData: parameters
            };
            const jsonString = angular.toJson(jsonWrapper);
            return $http.put(putUrl, jsonString);
        }

        function getApplicationDataPromise(applicationName, schemaId, parameters) {
            const url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters);
            return $http.get(url);
        };


        function getApplicationWithInitialDataPromise(applicationName, schemaId, parameters, initialData) {
            //TODO: refactor this code to pass data on body instead of query string
            if (initialData && !isString(initialData)) {               
                initialData = angular.toJson(initialData);
            }
            const url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters, initialData);
            return $http.post(url, initialData);
        };

        const service = {
            cancelDetail,
            getApplicationUrl,
            getPostPromise,
            invokeOperation,
            getApplicationDataPromise,
            getApplicationWithInitialDataPromise,
            submitData,
            save
        };
        return service;

    }
})(angular);



