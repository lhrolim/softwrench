(function (angular) {
    'use strict';

    function service($rootScope, $timeout, $log, associationService, crudContextHolderService, schemaService, searchService) {

        function showModal(target, element) {
            $timeout(function() {
                var modals = $('[data-attribute="{0}"]'.format(target), element);
                modals.draggable();
                modals.modal('show');
            }, 0, false);
        };

        function init(bodyElement, scope) {

        };

        function unblock(displayable) {

        };

        function block(displayable) {

        };

        /**
         * 
         * @param {} fieldMetadata 
         * @param {} datamap 
         * @param {} datamapId used to diferentiate compositions entries
         * @param {} newValue 
         * @returns {} 
         */
        function refreshFromAttribute(fieldMetadata, datamap, datamapId, newValue) {
            var log = $log.getInstance("cmplookup#refreshFromAttribute", ["association", "lookup"]);

            var associationKey = fieldMetadata.associationKey;

            associationService.getLabelText(associationKey, newValue, {
                hideDescription: fieldMetadata.hideDescription,
                allowTransientValue: fieldMetadata.rendererParameters["allowcustomvalue"] === "true",
                isEager: !!fieldMetadata.providerAttribute
            })
            .then(function (label) {
                var key = fieldMetadata.applicationPath;
                if (datamapId) {
                    key += datamapId;
                }
                key = replaceAll(key, "\\.", "_");

                log.debug("setting lookup {0} to {1}".format(key, label));
                var el = $("input[data-displayablepath=" + key + "]");
                if (el.length === 0) {
                    log.warn("lookup {0} not found".format(key));
                }
                el.typeahead("val", label);
                log.debug("setting", associationKey, "to", label);
            });
        }

        function updateLookupObject(scope, fieldMetadata, searchValue, searchDatamap) {
            scope.lookupObj = scope.lookupObj || {};

            scope.lookupObj.code = searchValue;
            scope.lookupObj.fieldMetadata = fieldMetadata;

            // fieldMetadata.schema will be `null` for eageroptions.lookup
            if (fieldMetadata.schema) {
                scope.lookupObj.application = fieldMetadata.schema.rendererParameters["application"];
                scope.lookupObj.schemaId = fieldMetadata.schema.rendererParameters["schemaId"];
            }

            var searchObj = {};
            searchObj.addPreSelectedFilters = true;
            var lookupAttribute = fieldMetadata.schema ? fieldMetadata.schema.rendererParameters["attribute"] : null;
            if (lookupAttribute != null) {
                searchObj[lookupAttribute] = searchValue;
            } else if (fieldMetadata.target != null) {
                searchObj[fieldMetadata.target] = searchValue;
            }

            associationService.updateDependentAssociationValues(scope, searchDatamap, scope.lookupObj, this.handleMultipleLookupOptionsFn, searchObj);
            //to avoid circular dependency
            scope.$emit("sw_resetFocusToCurrent", scope.schema, fieldMetadata.attribute);
            //            focusService.resetFocusToCurrent(scope.schema, fieldMetadata.attribute);
        };

        function handleMultipleLookupOptionsFn(result, lookupObj, scope, datamap) {
            var log = $log.get("cmplookup#handleMultipleLookupOptionsFn");

            var associationResult = result;
            lookupObj.schema = associationResult.associationSchemaDefinition;
            lookupObj.options = associationResult.associationData;

            if (hasSingleElement(associationResult.associationData)) {
                var firstOption = lookupObj.options[0];
                var firstElementEqualsCode = firstOption.value != null && lookupObj.code != null && firstOption.value.toUpperCase() == lookupObj.code.toUpperCase();
                var attribute = lookupObj.fieldMetadata.attribute;
                var datamapIsChanging = datamap[attribute] != firstOption.value;
                if (firstElementEqualsCode && datamapIsChanging) {
                    log.debug("exact match {0} found for modal/ avoid opening it".format(lookupObj.code));
                    associationService.updateUnderlyingAssociationObject(lookupObj.fieldMetadata, firstOption, scope);
                    //this will prevent modal from opening, since there´s just one option available
                    if (lookupObj.item) {
                        lookupObj.item[attribute] = firstOption.value;
                    } else {
                        datamap[attribute] = firstOption.value;
                    }
                    return true;
                }
            }
            lookupObj.modalPaginationData = {};
            lookupObj.modalPaginationData.pageCount = associationResult.pageCount;
            lookupObj.modalPaginationData.pageNumber = associationResult.pageNumber;
            lookupObj.modalPaginationData.pageSize = associationResult.pageSize;
            lookupObj.modalPaginationData.totalCount = associationResult.totalCount;
            lookupObj.modalPaginationData.selectedPage = associationResult.pageNumber;
            
            if (associationResult.searchDTO && associationResult.searchDTO.searchParams && associationResult.searchDTO.searchValues) {
                var result = searchService.buildSearchDataAndOperations(associationResult.searchDTO.searchParams, associationResult.searchDTO.searchValues);
                lookupObj.searchData = result.searchData;
                lookupObj.searchOperator = result.searchOperator;
            }
          
            //TODO: this should come from the server side
            lookupObj.modalPaginationData.paginationOptions = associationResult.paginationOptions || [10, 30, 100];
            showModal(lookupObj.fieldMetadata.target, lookupObj.element);
            return false;
        };

        function displayLookupModal(element) {
            showModal(element);
        };

        var api = {
            unblock: unblock,
            block: block,
            refreshFromAttribute: refreshFromAttribute,
            init: init,
            updateLookupObject: updateLookupObject,
            handleMultipleLookupOptionsFn: handleMultipleLookupOptionsFn,
            displayLookupModal: displayLookupModal
        };

        return api;

    }

    service.$inject = ['$rootScope', '$timeout', '$log', 'associationService', 'crudContextHolderService', 'schemaService', 'searchService'];

    angular.module("sw_lookup").factory('cmplookup', service);

})(angular);