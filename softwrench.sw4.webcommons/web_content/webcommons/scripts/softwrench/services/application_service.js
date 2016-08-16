﻿
(function (angular) {
    'use strict';



    angular.module('webcommons_services').factory('applicationService', ["$q", '$http', '$rootScope', 'contextService', "crudContextHolderService", "userPreferencesService", "alertService", "checkpointService", applicationService]);

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

    function applicationService($q, $http, $rootScope, contextService, crudContextHolderService, userPreferencesService, alertService, checkpointService) {

        var buildApplicationURLForBrowser = function (applicationName, parameters) {
            var crudUrl = $(routes_homeurl)[0].value;
            var currentModule = contextService.retrieveFromContext('currentmodule');
            var currentMetadata = contextService.retrieveFromContext('currentmetadata');

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
         * @returns {} 
         */
        function save(parameters) {
            var deferred = $q.defer();

            var successCallBack = function (data) {
                deferred.resolve(data);
            }

            var failureCallback = function (data) {
                deferred.reject(data);
            }

            parameters = parameters || {
                refresh:false
            };

            var isComposition = crudContextHolderService.getActiveTab() !== null;
            var dispatchedByModal = parameters.dispatchedByModal;

            if (dispatchedByModal == undefined) {
                dispatchedByModal = crudContextHolderService.isShowingModal();
            }



            //TODO: refactor it entirely to use promises instead
            $rootScope.$broadcast("sw_submitdata", {
                successCbk: successCallBack,
                failureCbk: failureCallback,
                isComposition: isComposition,
                compositionData: parameters.compositionData,
                dispatcherComposition: parameters.dispatcherComposition,
                nextSchemaObj: parameters.nextSchemaObj,
                dispatchedByModal: dispatchedByModal,
                refresh: parameters.refresh
            });

            return deferred.promise;
        }

        /// <summary>
        ///  refactor to use promises
        /// </summary>
        /// <param name="successCallBack"></param>
        /// <param name="failureCallback"></param>
        /// <param name="extraParameters"></param>
        function submitData(successCallBack, failureCallback, extraParameters) {

            extraParameters = extraParameters || {};
            var isComposition = extraParameters.isComposition;
            var nextSchemaObj = extraParameters.nextSchemaObj;
            var refresh = extraParameters.refresh;

            //TODO: refactor it entirely to use promises instead
            $rootScope.$broadcast("sw_submitdata", {
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

            var pageSize = userPreferencesService.getSchemaPreference("pageSize", applicationName, schemaId);
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
            var postUrl = url("/api/data/" + applicationName);

            var jsonWrapper = {
                json: datamap,
                requestData: parameters
            }

            var jsonString = angular.toJson(jsonWrapper);
            return $http.post(postUrl, jsonString);
        };

        function invokeOperation(applicationName, schemaId, operation, datamap, extraParameters) {
            var parameters = extraParameters ? extraParameters : {};
            parameters.Operation = operation;

            parameters = fillApplicationParameters(parameters, applicationName, schemaId, null);
            var putUrl = url("/api/data/" + applicationName);

            var jsonWrapper = {
                json: datamap,
                requestData: parameters
            }

            var jsonString = angular.toJson(jsonWrapper);
            return $http.put(putUrl, jsonString);
        }

        function getApplicationDataPromise(applicationName, schemaId, parameters) {
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters);
            return $http.get(url);
        };


        function getApplicationWithInitialDataPromise(applicationName, schemaId, parameters, initialData) {
            //TODO: refactor this code to pass data on body instead of query string
            if (initialData && !isString(initialData)) {               
                initialData = angular.toJson(initialData);
            }
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters, initialData);
            return $http.post(url, initialData);
        };

        var service = {
            cancelDetail: cancelDetail,
            getApplicationUrl: getApplicationUrl,
            getPostPromise: getPostPromise,
            invokeOperation: invokeOperation,
            getApplicationDataPromise: getApplicationDataPromise,
            getApplicationWithInitialDataPromise: getApplicationWithInitialDataPromise,
            submitData: submitData,
            save: save
        };

        return service;

    }
})(angular);



