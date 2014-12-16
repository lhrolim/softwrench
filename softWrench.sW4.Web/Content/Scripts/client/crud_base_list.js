//idea took from  https://www.exratione.com/2013/10/two-approaches-to-angularjs-controller-inheritance/
function BaseList($scope, formatService, expressionService, searchService) {

    $scope.getFormattedValue = function (value, column, datamap) {
        var formattedValue = formatService.format(value, column, datamap);
        if (formattedValue == "-666") {
            //this magic number should never be displayed! 
            //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
            return null;
        }
        return formattedValue;
    };

    $scope.searchOperations = function () {
        return searchService.searchOperations();
    };

    $scope.getDefaultOperator = function () {
        return searchService.defaultSearchOperation();
    };

    $scope.handleDefaultValue = function (data, column) {
        var key = column.target ? column.target : column.attribute;

        if (column.defaultValue != null && data[key] == null) {
            if (column.enableDefault != null && expressionService.evaluate(column.enableDefault, data)) {
                data[key] = column.defaultValue;
            }
        }
    };

    $scope.getOperator = function (columnName) {
        var searchOperator = $scope.searchOperator;
        if (searchOperator != null && searchOperator[columnName] != null) {
            return searchOperator[columnName];
        }
        return searchService.getSearchOperation(0);
    };

    $scope.getSearchIcon = function (columnName) {
        var showSearchIcon = $scope.schema.properties["list.advancedfilter.showsearchicon"] != "false";
        var operator = $scope.getOperator(columnName);
        return showSearchIcon ? operator.symbol : "";
    };

    $scope.GetAssociationOptions = function (fieldMetadata, forfilter) {
        if (fieldMetadata.type == "OptionField") {
            return $scope.GetOptionFieldOptions(fieldMetadata, forfilter);
        }
        $scope.$parent.associationOptions = instantiateIfUndefined($scope.$parent.associationOptions);
        return $scope.$parent.associationOptions[fieldMetadata.associationKey];
    };

    $scope.GetOptionFieldOptions = function (optionField, forfilter) {
        if (optionField.providerAttribute == null) {
            return optionField.options;
        }
        optionField.jscache = instantiateIfUndefined(optionField.jscache);
        if (optionField.jscache.providerOptions) {
            return optionField.jscache.providerOptions;
        }
        $scope.$parent.associationOptions = instantiateIfUndefined($scope.$parent.associationOptions);
        var associationOptions = $scope.$parent.associationOptions[optionField.providerAttribute];
        if (forfilter || optionField.addBlankOption) {
            associationOptions.unshift({
                label: "",
                value: ""
            });
        }
        optionField.jscache.providerOptions = associationOptions;
        return associationOptions;
    };

    $scope.shouldShowHeaderLabel = function (column) {
        return (column.type == "ApplicationFieldDefinition" || column.type == "OptionField") && column.rendererType != "color" && column.rendererType != "icon";
    };

    $scope.shouldShowHeaderFilter = function (column) {
        return $scope.shouldShowHeaderLabel(column) && !column.rendererParameters["hidefilter"];
    };

}