
(function (angular, $) {
    "use strict";

    let getWoDatamap, getWoSchema;

    const woDetailSchema = "workpackagesimplecomposition";

    class fsWorkPackageRelatedRecordsService {

        constructor($rootScope, $http, $q, redirectService, validationService, schemaCacheService, crudContextHolderService, submitService, compositionService) {
            this.$rootScope = $rootScope;
            this.$http = $http;
            this.$q = $q;
            this.redirectService = redirectService;
            this.validationService = validationService;
            this.schemaCacheService = schemaCacheService;
            this.crudContextHolderService = crudContextHolderService;
            this.submitService = submitService;
            this.compositionService = compositionService;

            getWoDatamap = function(woId) {
                const params = {
                    id: woId,
                    key: { schemaId: woDetailSchema, mode: "input", platform: "web" },
                    customParameters: {},
                    printMode: null
                }
                const urlToCall = url("/api/data/workorder?" + $.param(params));
                return $http.get(urlToCall).then(response => {
                    return response.data.resultObject;
                });
            }

            getWoSchema = function() {
                return schemaCacheService.fetchSchema("workorder", woDetailSchema);
            }
        }

        rowClick(item) {
            const params = {
                userid: item["#relatedreckey"],
                siteid: item["#siteid"],
                saveHistoryReturn: true
            };
            return this.redirectService.goToApplicationView("workorder", "editdetail", "input", null, params);
        }

        save(item) {
            if (this.validationService.validateCurrent("#modal").length > 0) {
                return this.$q.when(null);
            }
            const woId = this.crudContextHolderService.rootDataMap()["#workorder_.workorderid"];
            const promises = [];
            const saveDatamap = {
                "#isDirty": true,
                _iscreation: true,
                relatedrecclass: "WORKORDER",
                relatedreckey: item["wonum"],
                relatedrecorgid: item["relatedworkorder_.orgid"],
                relatedrecsiteid: item["relatedworkorder_.siteid"]
            }

            promises.push(getWoSchema());
            promises.push(getWoDatamap(woId));
            return this.$q.all(promises).then((results) => {
                const woDatamap = results[1];
                const woSchema = results[0];
                woDatamap["relatedrecord_"] = [saveDatamap];
                woDatamap["class"] = "WORKORDER";
                const params = {
                    compositionData: new CompositionOperation("crud_create", "relatedrecord_", saveDatamap),
                    dispatchedByModal: false,
                    refresh: true,
                    successMessage: "Related Work Order successfully created."
                }
                return this.submitService.submit(woSchema, woDatamap, params)
                    .then((data) => {
                        this.$rootScope.$broadcast(JavascriptEventConstants.CompositionRefreshPage, data, false, false);
                    });
            });
        }
    }


    fsWorkPackageRelatedRecordsService["$inject"] = ["$rootScope", "$http", "$q", "redirectService", "validationService", "schemaCacheService", "crudContextHolderService", "submitService", "compositionService"];

    angular.module("sw_layout").service("fsWorkPackageRelatedRecordsService", fsWorkPackageRelatedRecordsService);

})(angular, jQuery);