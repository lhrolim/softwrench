
(function (angular) {
	"use strict";

	function crudSearchService($rootScope, $log, $q, $http, schemaCacheService, applicationService, sidePanelService) {
		//#region Utils
	    var log = $log.getInstance("sw4.crudSearchService");

	    // workaround - if the schemaCacheService.getCachedSchema is used before $http.get(applicationService.getApplicationUrl(...))
	    // the null schema result is considered cached and the server don't build the correct schema after that
	    // only the key is cached here to avoid invoke schemaCacheService.getCachedSchema first
	    // TODO Fix schemaCacheService.getCachedSchema + $http.get(applicationService.getApplicationUrl(...))
	    const schemacache = {};

		//#endregion

		//#region Public methods

		function search(schema, datamap, searchOperator) {
		    const args = [schema, datamap];
			if (searchOperator) {
			    args.push(searchOperator);
			}
			log.debug("Crud search ...");
			$rootScope.$broadcast("sw.crud.search", args);
		}

		function getSearchSchemaId(applicationName, renderedSchema) {
		    if (!applicationName || !renderedSchema) {
		        log.debug("no applicationName ({0}) or renderedSchema ({1})".format(applicationName, renderedSchema));
		        return null;
		    }
		    const searchSchemaid = renderedSchema.properties ? renderedSchema.properties["search.schemaid"] : null;
		    if (!searchSchemaid) {
		        log.debug("no searchSchemaid on applicationName ({0}) and renderedSchema ({1})".format(applicationName, renderedSchema));
		    }
		    return searchSchemaid;
		}

		function getSearchSchema(applicationName, schemaId) {
		    if (!applicationName || !schemaId) {
		        log.debug("getSearchSchema  - no applicationName ({0}) or schemaId ({1})".format(applicationName, schemaId));
		        return $q.when(null);
		    }

		    if (schemacache[applicationName + "." + schemaId]) {
		        log.debug("getSearchSchema  - cache hit on applicationName ({0}) and  schemaId ({1})".format(applicationName, schemaId));
		        return $q.when(schemaCacheService.getCachedSchema(applicationName, schemaId));
		    }
		    const redirectUrl = applicationService.getApplicationUrl(applicationName, schemaId, "input");
		    return $http.get(redirectUrl).then(function (httpResponse) {
		        log.debug("getSearchSchema - server response on applicationName ({0}) and  schemaId ({1})".format(applicationName, schemaId));
		        const schema = httpResponse.data.schema;
		        schemaCacheService.addSchemaToCache(schema);
		        schemacache[applicationName + "." + schemaId] = true;
		        return schema;
		    });
		}

		function updateCrudSearchSidePanel(panelid, searchSchema) {
		    if (!searchSchema) {
		        sidePanelService.hide(panelid);
                return;
            }

		    sidePanelService.setTitle(panelid, searchSchema.title);
		    const handleWidth = searchSchema.properties ? searchSchema.properties["search.panelwidth"] : null;
		    sidePanelService.setHandleWidth(panelid, handleWidth);
		    sidePanelService.show(panelid);
        }

		//#endregion

		//#region Service Instance
		return {
		    search,
		    getSearchSchemaId,
		    getSearchSchema,
		    updateCrudSearchSidePanel
		};
		//#endregion
	}

	//#region Service registration

	angular.module("sw_layout").service("crudSearchService", ["$rootScope", "$log", "$q", "$http", "schemaCacheService", "applicationService", "sidePanelService", crudSearchService]);

	//#endregion

})(angular);