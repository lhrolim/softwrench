﻿modules.webcommons.factory('applicationService', function ($http) {

    return {

        getApplicationUrl: function (applicationName, schemaId, mode, title, parameters, jsonData) {
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
        },


        invokeOperation: function (applicationName, schemaId, operation, datamap, extraParameters) {
            var fields = datamap;
            if (datamap.fields) {
                fields = datamap.fields;
            }

            var parameters = extraParameters ? extraParameters : {};
            parameters.Operation = operation;
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters);
            return $http.put(url, angular.toJson(fields));
        },


        getApplicationDataPromise: function (applicationName, schemaId, parameters) {
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters);
            return $http.get(url);
        },


        getApplicationWithInitialDataPromise: function (applicationName, schemaId, parameters, initialData) {
            if (initialData && !isString(initialData)) {
                if (initialData.fields) {
                    initialData = initialData.fields;
                }
                initialData = angular.toJson(initialData);
            }
            var url = this.getApplicationUrl(applicationName, schemaId, null, null, parameters, initialData);
            return $http.post(url,initialData);
        },
    };


});


