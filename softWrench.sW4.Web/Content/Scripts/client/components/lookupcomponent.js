var app = angular.module('sw_layout');

app.factory('cmplookup', function ($rootScope, $timeout, $log, associationService) {

    var showModal = function (element) {
        var modals = $('[data-class="lookupModal"]', element);
        modals.draggable();
        modals.modal('show');
    };

    return {

        unblock: function (displayable) {

        },

        block: function (displayable) {

        },

        refreshFromAttribute: function (fieldMetadata, scope) {
            var log = $log.getInstance('cmplookup#refreshFromAttribute');
            if (scope.associationOptions == null) {
                //this scenario happens when a composition has lookup-associations on its details,
                //but the option list has not been fetched yet
                scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                scope.lookupAssociationsCode[fieldMetadata.attribute] = null;
                log.debug('cleaning up association code/description');
                return;
            }

            var options = scope.associationOptions[fieldMetadata.associationKey];
            var optionValue = scope.datamap[fieldMetadata.target];
            scope.lookupAssociationsCode[fieldMetadata.attribute] = optionValue;
            if (optionValue == null) {
                scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
            }

            if (options == null || options.length <= 0) {
                //it should always be lazy loaded... why is this code even needed?
                return;
            }

            var optionSearch = $.grep(options, function (e) {
                return e.value == optionValue;
            });

            var valueToSet = optionSearch != null && optionSearch.length > 0 ? optionSearch[0].label : null;
            scope.lookupAssociationsDescription[fieldMetadata.attribute] = valueToSet;
        },

        init: function (bodyElement, scope) {

        },

        
        updateLookupObject: function (scope, fieldMetadata) {
            if (scope.lookupObj == null) {
                scope.lookupObj = {};
            }
            var code = scope.lookupAssociationsCode[fieldMetadata.attribute];
            scope.lookupObj.code = code;
            scope.lookupObj.fieldMetadata = fieldMetadata;
            scope.lookupObj.application = fieldMetadata.schema.rendererParameters["application"];
            scope.lookupObj.schemaId = fieldMetadata.schema.rendererParameters["schemaId"];

            var searchObj = {};
            var lookupAttribute = fieldMetadata.schema.rendererParameters["attribute"];
            if (lookupAttribute != null) {
                searchObj[lookupAttribute] = code;
            } else if (fieldMetadata.target != null) {
                searchObj[fieldMetadata.target] = code;
            }

            associationService.updateDependentAssociationValues(scope, scope.datamap, scope.lookupObj, this.handleMultipleLookupOptionsFn, searchObj);

        },

        handleMultipleLookupOptionsFn: function (result, lookupObj, scope, datamap) {
            var associationResult = result[lookupObj.fieldMetadata.associationKey];
            lookupObj.schema = associationResult.associationSchemaDefinition;
            lookupObj.options = associationResult.associationData;
            if (Object.keys(result).length == 1 &&
                result[lookupObj.fieldMetadata.associationKey] != null &&
                result[lookupObj.fieldMetadata.associationKey].associationData != null &&
                result[lookupObj.fieldMetadata.associationKey].associationData.length == 1) {
                if (lookupObj.options[0].value == lookupObj.code &&
                    datamap[lookupObj.fieldMetadata.attribute] != lookupObj.options[0].value) {
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
            showModal(lookupObj.element);
            return false;
        },

        displayLookupModal: function (element) {
            showModal(element);
        }



    }

});


