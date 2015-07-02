modules.webcommons.factory('applicationService', function ($http) {

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


