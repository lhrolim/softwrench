(function (angular) {
    "use strict";

    angular.module("sw_lookup").directive("lookupInput", ["$q", "lookupService", "contextService", 'expressionService', 'cmpfacade',
        'dispatcherService', 'modalService', 'compositionCommons', 'i18NService',
        function ($q, lookupService, contextService, expressionService, cmpfacade, dispatcherService, modalService, compositionCommons, i18NService) {

            const directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl('/Content/modules/lookup/templates/lookupinput.html'),
                scope: {
                    datamap: '=',
                    parentdata: '=',
                    schema: '=',
                    fieldMetadata: '=',
                    panelid: "@",
                    displayablepath: '@',
                    mode: '@'
                },

                link: function (scope) {

                    scope.lookupObj = new LookupDTO(scope.fieldMetadata);

                    if (scope.fieldMetadata &&
                        scope.fieldMetadata.rendererParameters &&
                        scope.fieldMetadata.rendererParameters.largemodal === "true") {
                        scope.largemodal = "largemodal";
                    } else {
                        scope.largemodal = "";
                    }

                    scope.vm = {
                        isSearching: false
                    };
                    scope.$name = "lookupinput";

                    scope.getPlaceholderText = function (fieldMetadata) {
                        return i18NService.getI18nPlaceholder(fieldMetadata);
                    }

                    scope.showCustomModal = function (fieldMetadata, schema, datamap) {
                        if (fieldMetadata.rendererParameters['schema'] == undefined) {
                            return $q.reject("schema must be defined for custom modal");
                        }
                        const service = fieldMetadata.rendererParameters['onsave'];
                        var savefn = $q.when();

                        if (service != null) {
                            const servicepart = service.split('.');
                            savefn = $q.when(dispatcherService.loadService(servicepart[0], servicepart[1]));
                        }
                        const onloadservice = fieldMetadata.rendererParameters['onload'];
                        let onloadfn = $q.when(null);

                        if (onloadservice != null) {
                            const onloadservicepart = onloadservice.split('.');
                            onloadfn = $q.when(dispatcherService.loadService(onloadservicepart[0], onloadservicepart[1]));
                        }
                        const modaldatamap = onloadfn(datamap, fieldMetadata.rendererParameters['schema'], fieldMetadata);

                        const properties = (() => {
                            const props = {};
                            const cssclass = fieldMetadata.rendererParameters["cssclass"];
                            if (!!cssclass) props.cssclass = cssclass;
                            const title = fieldMetadata.rendererParameters["title"];
                            if (!!title) props.title = title;
                            return props;
                        })();

                        return modalService.showPromise(fieldMetadata.rendererParameters['schema'], modaldatamap, properties, datamap, schema).then(selecteditem => {
                            //TODO: document this custom function
                            return savefn(datamap, fieldMetadata.rendererParameters['schema'], selecteditem, fieldMetadata);
                        });
                    };


                    scope.$on(JavascriptEventConstants.AssociationResolved, function (event, panelid) {
                        if (panelid != scope.panelid && !(panelid == null && scope.panelid === "")) {
                            //keep != to avoid errors
                          return;
                        }
                        scope.lookupObj = new LookupDTO(scope.fieldMetadata);
                    
                    });


                    scope.showLookupModal = function () {
                        //this will include (link) the directive defined on lookupModal.js on the screen
                        //check the ng-if on the lookupinput.html for more information
                        scope.loadModalWrappers = {
                         [scope.fieldMetadata.attribute]: true
                        };

                        const fieldMetadata = scope.fieldMetadata;

                        if (fieldMetadata.rendererType === "modal") {
                            return this.showCustomModal(fieldMetadata, scope.schema, scope.datamap);
                        }

                        var searchDatamap = scope.datamap;

                        scope.lookupObj = scope.lookupObj || new LookupDTO(fieldMetadata);

                        if (scope.parentdata) {
                            scope.lookupObj.item = scope.datamap;
                            searchDatamap = compositionCommons.buildMergedDatamap(scope.datamap, scope.parentdata);
                        }

                        const overrideschema = (scope.fieldMetadata.rendererParameters["lookup.usescopeschema"] || searchDatamap["#datamaptype"] === "compositionitem")? scope.schema : null;

                        return lookupService.initLookupModal(scope.lookupObj, scope.datamap, searchDatamap, overrideschema)
                            .then(lookupObj => {
                                scope.lookupObj = lookupObj;
                            });
                    }


                }


            };
            return directive;

        }]);

})(angular);

