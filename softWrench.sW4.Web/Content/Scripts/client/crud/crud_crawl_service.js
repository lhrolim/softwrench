(function (angular) {
	"use strict";

	function crudCrawlService($rootScope, crudContextHolderService, contextService) {
		//#region Utils
		function crawlAction(direction) {
			const data = crawlData(direction);
			if (!data) {
			    return;
			}
		    data.customParameters = data.customParameters || {};
		    data.customParameters.skipDirtyMessage = true;
			$rootScope.$broadcast("sw_navigaterequest", data.applicationname, data.schemaid, "input", null, { id: data.id, popupmode: "none", customParameters: data.customParameters });
		}
		//#endregion

		//#region Public methods
		function crawlData(direction) {
			const schema = crudContextHolderService.currentSchema();
			const value = contextService.fetchFromContext("crud_context", true);
			var item = direction === 1 ? value.detail_previous : value.detail_next;

			if (!item) return null;

			// If the detail crawl has a custom param, we need to get it from the pagination list
			const customParameters = {};
			if (schema.properties && schema.properties["detail.crawl.customparams"]) {
				// Get the next/previous record that the params will be coming from
				const record = value.previousData.filter(function (obj) {
					return obj[schema.idFieldName] === item.id;
				}); // TODO: If the record is null (item not on page in previous data)????
				const customparamAttributes = schema.properties["detail.crawl.customparams"].replace(" ", "").split(",");
				for (let param in customparamAttributes) {
					if (!customparamAttributes.hasOwnProperty(param)) {
						continue;
					}
					customParameters[param] = {};
					customParameters[param]["key"] = customparamAttributes[param];
					customParameters[param]["value"] = record[0][customparamAttributes[param]];
				}
			}
			return {
				schemaid: item.detailSchemaId || schema.schemaId,
				applicationname: item.application || schema.applicationName,
				id: item.id,
				customParameters: customParameters
			}
		}

		function forwardAction() {
		    crawlAction(0);
		}

		function backAction() {
			crawlAction(1);
		}

		//#endregion

		//#region Service Instance
		const service = {
			crawlData,
			forwardAction,
			backAction
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("sw_layout").factory("crudCrawlService", ["$rootScope", "crudContextHolderService", "contextService", crudCrawlService]);

	//#endregion

})(angular);