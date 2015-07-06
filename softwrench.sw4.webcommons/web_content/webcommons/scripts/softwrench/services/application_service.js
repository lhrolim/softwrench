
(function () {
    'use strict';

    angular.module('webcommons_services').factory('applicationService', ['$http', '$rootScope', applicationService]);

    function applicationService($http, $rootScope) {

        var service = {
            getApplicationUrl: getApplicationUrl,
            invokeOperation: invokeOperation,
            getApplicationDataPromise: getApplicationDataPromise,
            getApplicationWithInitialDataPromise: getApplicationWithInitialDataPromise,
            submitData: submitData
        };

        return service;


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
            $rootScope.$broadcast("sw_submitdata",{
                successCbk: successCallBack,
                failureCbk: failureCallback,
                isComposition: isComposition,
                nextSchemaObj: nextSchemaObj,
                refresh: refresh

            });
        }

        function getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            parameters.key = {
                schemaId: schemaId,
                mode: mode,
                platform: platform()
            };
            parameters.currentSchemaKey = schemaId;


            if (parameters.popupmode == "browser") {
                return buildApplicationURLForBrowser(applicationName, parameters);
            }
            if (title != null && title.trim() != "") {
                parameters.title = title;
            }
            if (jsonData == undefined) {
                return url("/api/data/" + applicationName + "?" + $.param(parameters));
            } else {
                parameters.application = applicationName;
                return url("/api/generic/ExtendedData/OpenDetailWithInitialData" + "?" + $.param(parameters));
            }
        };

        function invokeOperation(applicationName, schemaId, operation, datamap, extraParameters) {
            var fields = datamap;
            if (datamap.fields) {
                fields = datamap.fields;
            }

            var parameters = extraParameters ? extraParameters : {};
            parameters.Operation = operation;
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters);
            return $http.put(url, angular.toJson(fields));
        }

        function getApplicationDataPromise(applicationName, schemaId, parameters) {
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters);
            return $http.get(url);
        };


        function getApplicationWithInitialDataPromise(applicationName, schemaId, parameters, initialData) {
            if (initialData && !isString(initialData)) {
                if (initialData.fields) {
                    initialData = initialData.fields;
                }
                initialData = angular.toJson(initialData);
            }
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters, initialData);
            return $http.post(url, initialData);
        };

    }
})();



