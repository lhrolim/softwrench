(function (angular) {
    'use strict';


    service.$inject = ['$rootScope', '$timeout', '$log', 'associationService'];

    angular.module("sw_lookup").factory('cmplookup', service);



    function service($rootScope, $timeout, $log, associationService) {


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


        function showModal(target, element) {
            var modals = $('[data-attribute="{0}"]'.format(target), element);
            modals.draggable();
            modals.modal('show');
        };

        function init(bodyElement, scope) {

        };

        function unblock(displayable) {

        };

        function block(displayable) {

        };

        function refreshFromAttribute(fieldMetadata, scope) {
            var log = $log.getInstance('cmplookup#refreshFromAttribute');
            var target = fieldMetadata.attribute;


            if (scope.associationOptions == null) {

                if (scope.lookupAssociationsDescription) {
                    //this scenario happens when a composition has lookup-associations on its details,
                    //but the option list has not been fetched yet
                    scope.lookupAssociationsDescription[target] = null;
                    scope.lookupAssociationsCode[target] = null;
                    log.debug('cleaning up association code/description');
                }
                return;
            }

            var options = scope.associationOptions[fieldMetadata.associationKey];
            if (options && options.length === 1) {
                var label = associationService.getLabelText(options[0], fieldMetadata.hideDescription);
                $("input[data-association-key=" + fieldMetadata.associationKey + "]").typeahead('val', label);
                return;
            }


            var optionValue = scope.datamap[fieldMetadata.target];
            scope.lookupAssociationsCode[target] = optionValue;

            

            log.debug('setting lookupassociationCode {0} to {1}'.format(target, optionValue));
            if (optionValue == null) {
                scope.lookupAssociationsDescription[target] = null;
            }

            if (options == null || options.length <= 0) {
                //it should always be lazy loaded... why is this code even needed?
                return;
            }

            var optionSearch = $.grep(options, function (e) {
                return e.value == optionValue;
            });

            var valueToSet = optionSearch != null && optionSearch.length > 0 ? optionSearch[0].label : null;
            scope.lookupAssociationsDescription[target] = valueToSet;
            log.debug('setting lookupassociationdescription {0} to {1} '.format(target, valueToSet));
        }

        function updateLookupObject(scope, fieldMetadata, searchValue, searchDatamap) {


            if (scope.lookupObj == null) {
                scope.lookupObj = {};
            }

            scope.lookupObj.code = searchValue;
            scope.lookupObj.fieldMetadata = fieldMetadata;
            scope.lookupObj.application = fieldMetadata.schema.rendererParameters["application"];
            scope.lookupObj.schemaId = fieldMetadata.schema.rendererParameters["schemaId"];

            var searchObj = {};
            var lookupAttribute = fieldMetadata.schema.rendererParameters["attribute"];
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

            var associationResult = result[lookupObj.fieldMetadata.associationKey];
            lookupObj.schema = associationResult.associationSchemaDefinition;
            lookupObj.options = associationResult.associationData;
            if (Object.keys(result).length == 1 && hasSingleElement(associationResult.associationData)) {
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
            //TODO: this should come from the server side
            lookupObj.modalPaginationData.paginationOptions = [10, 30, 100];
            showModal(lookupObj.fieldMetadata.target, lookupObj.element);
            return false;
        };

        function displayLookupModal(element) {
            showModal(element);
        };
    }
})(angular);



