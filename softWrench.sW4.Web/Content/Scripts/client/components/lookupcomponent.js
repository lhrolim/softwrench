var app = angular.module('sw_layout');
app.factory('cmplookup', function ($rootScope, $timeout) {
    return {
        unblock: function (displayable) {
        },
        block: function (displayable) {
        },
        refreshFromAttribute: function (fieldMetadata, scope) {
            if (scope.associationOptions == null) {
                //this scenario happens when a composition has lookup-associations on its details,
                //but the option list has not been fetched yet
                scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                scope.lookupAssociationsCode[fieldMetadata.attribute] = null;
                return;
            }
            var options = scope.associationOptions[fieldMetadata.associationKey];
            var optionValue = scope.datamap[fieldMetadata.target];
            scope.lookupAssociationsCode[fieldMetadata.attribute] = optionValue;
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
        }
    }
});
