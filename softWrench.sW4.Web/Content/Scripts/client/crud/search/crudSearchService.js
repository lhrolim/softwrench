
(function (angular) {
	"use strict";

	function crudSearchService($rootScope, $log) {
		//#region Utils
	    var log = $log.getInstance("sw4.crudSearchService");
		//#endregion

		//#region Public methods

		function search(schema, datamap, searchOperator) {
		    var args = [schema, datamap];
			if (searchOperator) {
			    args.push(searchOperator);
			}
			log.debug("Crud search ...");
			$rootScope.$broadcast("sw.crud.search", args);
		}

		//#endregion

		//#region Service Instance
		var service = {
			search: search
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("sw_layout").service("crudSearchService", ["$rootScope", "$log", crudSearchService]);

	//#endregion

})(angular);