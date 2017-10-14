(function (angular) {
    "use strict";

    angular.module("sw_lookup").directive("lookupInput", ["$q", "lookupService", "contextService", 'expressionService', 'cmpfacade',
        'dispatcherService', 'modalService', 'compositionCommons', 'i18NService', 'schemaService', 
        function ($q, lookupService, contextService, expressionService, cmpfacade, dispatcherService, modalService, compositionCommons, i18NService, schemaService) {

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

                    const fieldMetadata = scope.fieldMetadata;
                    scope.customModal = fieldMetadata.rendererType === "modal" || (fieldMetadata.rendererParameters && fieldMetadata.rendererParameters["lookup.mode"] === "modal");

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
                        if (!fieldMetadata.rendererParameters["schemaid"] || !fieldMetadata.rendererParameters["application"] ) {
                            return $q.reject("schema and application must be defined for custom modal");
                        }
                        const service = fieldMetadata.rendererParameters['onsave'];
                        let savefn = null;

                        if (service != null) {
                            const servicepart = service.split('.');
                            savefn = dispatcherService.loadService(servicepart[0], servicepart[1]);
                        }
                        const onloadservice = fieldMetadata.rendererParameters['onload'];
                        let onloadfn = null;

                        if (onloadservice != null) {
                            const onloadservicepart = onloadservice.split('.');
                            onloadfn = dispatcherService.loadService(onloadservicepart[0], onloadservicepart[1]);
                        }

                        const schemaId = fieldMetadata.rendererParameters["schemaid"];
                        const application = fieldMetadata.rendererParameters["application"];

                        return schemaService.getSchema(application, schemaId).then(fieldSchema => {
                            const props = {
                                cssclass: fieldMetadata.rendererParameters["cssclass"] || ""
                            };
                            const title = fieldMetadata.rendererParameters["title"];
                            if (!!title) props.title = title;

                            if (fieldMetadata.rendererParameters["largemodal"] === "true") {
                                props.cssclass = `${props.cssclass} largemodal`;
                                props.removecrudmodalclass = true;
                            }

                            const modaldatamap = onloadfn ? onloadfn(datamap, fieldSchema, fieldMetadata, props) : {};

                            return modalService.showPromise(fieldSchema, modaldatamap, props, datamap, schema).then(selecteditems => {
                                //TODO: document this custom function
                                return savefn && savefn(datamap, fieldSchema, selecteditems, fieldMetadata, schema);
                            });
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

                        if (scope.customModal) {
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

