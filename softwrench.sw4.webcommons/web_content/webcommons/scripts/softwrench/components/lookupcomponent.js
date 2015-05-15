var app = angular.module('sw_layout');

app.factory('cmplookup', function ($rootScope, $timeout, $log, associationService) {

    var showModal = function (target,element) {
        var modals = $('[data-attribute="{0}"]'.format(target), element);
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
            var target = fieldMetadata.attribute;
            if (scope.associationOptions == null) {
                //this scenario happens when a composition has lookup-associations on its details,
                //but the option list has not been fetched yet
                scope.lookupAssociationsDescription[target] = null;
                scope.lookupAssociationsCode[target] = null;
                log.debug('cleaning up association code/description');
                return;
            }

            var options = scope.associationOptions[fieldMetadata.associationKey];
            var optionValue = scope.datamap[fieldMetadata.target];
            scope.lookupAssociationsCode[target] = optionValue;
            log.debug('setting lookupassociationCode {0} to {1}'.format(target,optionValue));
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
        },

        init: function (bodyElement, scope) {

        },

        
        updateLookupObject: function (scope, fieldMetadata, searchValue) {
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
                if (lookupObj.options[0].value != null && lookupObj.code != null && lookupObj.options[0].value.toUpperCase() == lookupObj.code.toUpperCase() &&
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
            showModal(lookupObj.fieldMetadata.target,lookupObj.element);
            return false;
        },

        displayLookupModal: function (element) {
            showModal(element);
        }



    }

});


