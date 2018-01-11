﻿
(function (angular) {
    'use strict';




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

    function applicationService($q, $http, $rootScope, $injector, contextService, crudContextHolderService, userPreferencesService, alertService, submitServiceCommons) {

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


        const defaultSaveParams = {
            refresh: false,
            skipValidation: false
        }

        /**
         * 
         * @param {} parameters 
         * 
         * compositionData {CompositionOperation}
         * 
         * 
         * @returns {Promise} 
         */
        function save({ schema,compositionData, nextSchemaObj, dispatchedByModal, refresh, selecteditem, originalDatamap, datamap, extraparameters, operation, skipValidation, customurl } = defaultSaveParams) {

            if (dispatchedByModal == undefined) {
                dispatchedByModal = crudContextHolderService.isShowingModal();
            }

            const panelId = dispatchedByModal ? "#modal" : null;

            schema = schema || crudContextHolderService.currentSchema(panelId);
            datamap = datamap || crudContextHolderService.rootDataMap(panelId);

            //LUIZ -- TODO: review circular dependency
            return $injector.get('submitService').submit(schema, datamap, {
                compositionData,
                nextSchemaObj,
                dispatchedByModal,
                refresh,
                selecteditem,
                extraparameters,
                originalDatamap,
                skipValidation,
                operation,
                customurl
            });

        }


        function deleteCrud(confirmationMessage) {
            let confirmationPromise = $q.when();
            if (confirmationMessage) {
                confirmationPromise = alertService.confirm(confirmationMessage);
            }
            return confirmationPromise.then(r => {
                const schema = crudContextHolderService.currentSchema();
                const datamap = crudContextHolderService.rootDataMap();

                const idFieldName = schema.idFieldName;
                const applicationName = schema.applicationName;
                const id = datamap[idFieldName];
                var parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    parameters.mockmaximo = true;
                }
                parameters.applicationName = applicationName;
                parameters.id = id;
                parameters.platform = platform();
                parameters = submitServiceCommons.addSchemaDataToParameters(parameters, schema);
                const deleteParams = $.param(parameters);
                const deleteUrl = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                return $http.delete(deleteUrl)
                    .then(function (response) {
                        const data = response.data;
                        return $injector.get('redirectService').redirectFromServerResponse(data, "input");
                    });
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
                    parameters["SearchDTO"] = { addPreSelectedFilters: true }
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
            save,
            deleteCrud
        };
        return service;

    }
    angular.module('webcommons_services').service('applicationService', ["$q", '$http', '$rootScope', '$injector', 'contextService', "crudContextHolderService", "userPreferencesService", "alertService", "submitServiceCommons", applicationService]);

})(angular);



