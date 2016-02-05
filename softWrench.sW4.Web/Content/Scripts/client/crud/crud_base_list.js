(function (angular) {
    "use strict";

angular.module("sw_layout").controller("BaseList", BaseList);

//idea took from  https://www.exratione.com/2013/10/two-approaches-to-angularjs-controller-inheritance/
BaseList.$inject = ["$scope", "formatService", "expressionService", "searchService", "fieldService", "i18NService", "commandService", "crudContextHolderService", "gridSelectionService"];
function BaseList($scope, formatService, expressionService, searchService, fieldService, i18NService, commandService, crudContextHolderService, gridSelectionService) {

    $scope.isFieldHidden = function (application, fieldMetadata) {
        return fieldService.isFieldHidden($scope.datamap, application, fieldMetadata);
    };

    $scope.getFormattedValue = function (value, column, datamap) {
        var formattedValue = formatService.format(value, column, datamap);
        if (formattedValue == "-666") {
            //this magic number should never be displayed! 
            //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
            return null;
        }
        return formattedValue;
    };

    $scope.i18NOptionField = function (option, fieldMetadata, schema) {
        return i18NService.getI18nOptionField(option, fieldMetadata, schema);
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.isColumnEditable = function (column) {
        return column.rendererParameters['editable'] === "true";
    };

    $scope.isColumnUpdatable = function (column) {
        return this.isColumnEditable(column) || column.rendererParameters['updatable'] === "true";
    };

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.searchOperations = function () {
        return searchService.searchOperations();
    };

    $scope.shouldShowFilter = function (operation, filter) {
        if (!filter.type) {
            //legacy code for lookups
            var filterByDataType = filter.dataType == null || operation.datatype == null || operation.datatype.indexOf(filter.dataType) > -1;
            return (filter.rendererType == null || operation.renderType.indexOf(filter.rendererType) > -1) && (filterByDataType);
        }
        if (filter.type === "BaseMetadataFilter") {
            return operation.id.equalsAny("CONTAINS", "EQ", "NCONTAINS", "STARTWITH","GT", "LT", "ENDWITH", "BLANK");
        } else if (filter.type === "MetadataNumberFilter") {
            return operation.id.equalsAny("GT", "LT", "GTE", "LTE", "EQ", "NOTEQ", "BLANK");
        } else if (filter.type === "MetadataDateTimeFilter") {
            return operation.id.equalsAny("GT", "LT", "GTE", "LTE", "EQ", "NOTEQ", "BLANK");
        }

        return false;
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

    function getIconForOperator(operatorLocator, columnName) {
        var showSearchIcon = $scope.schema.properties["list.advancedfilter.showsearchicon"] !== "false";
        var operator = operatorLocator.bind($scope)(columnName);
        return showSearchIcon ? operator.symbol : "";
    }

    $scope.getDefaultSearchIcon = function () {
        return getIconForOperator($scope.getDefaultOperator);
    }

    $scope.getSearchIcon = function (columnName) {
        return getIconForOperator($scope.getOperator, columnName);
    };

    $scope.GetAssociationOptions = function (fieldMetadata, forfilter) {
        if (fieldMetadata.type === "OptionField") {
            return $scope.GetOptionFieldOptions(fieldMetadata, forfilter);
        }
        var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
        return crudContextHolderService.fetchEagerAssociationOptions(fieldMetadata.associationKey, contextData);
    };

    $scope.GetOptionFieldOptions = function (optionField, forfilter) {
        if (optionField.providerAttribute == null) {
            return optionField.options;
        }
        optionField.jscache = instantiateIfUndefined(optionField.jscache);
        if (optionField.jscache.providerOptions) {
            return optionField.jscache.providerOptions;
        }
        
        var associationOptions = crudContextHolderService.fetchEagerAssociationOptions(optionField.providerAttribute);
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

    $scope.showDetail = function (rowdm, column, forceEdition) {

        var mode = $scope.schema.properties['list.click.mode'];
        var popupmode = $scope.schema.properties['list.click.popupmode'];
        var schemaid = $scope.schema.properties['list.click.schema'];
        var fullServiceName = $scope.schema.properties['list.click.service'];
        var editDisabled = $scope.schema.properties['list.disabledetails'];
        var commandResult = null;

        var selectionModel = crudContextHolderService.getSelectionModel($scope.panelid);

        //force edition means that the user has clicked the edition icon, so regardless of the mode we need to open the details
        if (selectionModel.selectionMode && !forceEdition) {
            commandResult = null;
            if (fullServiceName != null) {
                commandResult =commandService.executeClickCustomCommand(fullServiceName, rowdm.fields, column, $scope.schema);
            };
            if (commandResult == undefined || commandResult !== false) {
                gridSelectionService.toggleSelection(rowdm, $scope.schema, $scope.panelid);
            }
            return;
        }


        if (popupmode === "report") {
            return;
        }

        if (mode && !mode.equalsAny('none', 'output', 'input')) {
            mode = expressionService.evaluate(mode, rowdm);
        }

        if ("true" === editDisabled && nullOrUndef(fullServiceName)) {
            return;
        }

        if (fullServiceName != null) {
            commandResult = commandService.executeClickCustomCommand(fullServiceName, rowdm.fields, column, $scope.schema);
            if (commandResult === false) {
                return;
            }
        };

        var id = rowdm.fields[$scope.schema.idFieldName];
        if (id == null || id == "-666") {
            window.alert('error id is null');
            return;
        }

        var applicationname = $scope.schema.applicationName;
        if (schemaid === '') {
            return;
        }
        if (schemaid == null) {
            schemaid = detailSchema();
        }

        // TODO: change both cases to redirectService.gotoApplicaiton 
        //if ("modal" === popupmode) {
        //    // TODO: pass id to search data from instead of datamap 
        //    redirectService.openAsModal(applicationname, schemaid, null, rowdm.fields);
        //    return;
        //}
        $scope.$emit("sw_renderview", applicationname, schemaid, mode, $scope.title, {
            id: id, popupmode: popupmode, customParameters: $scope.getCustomParameters($scope.schema, rowdm)
        });
    };

    $scope.getCustomParameters = function (schema, rowdm) {
        var customParams = {};
        if (schema.properties["list.click.customparams"]) {
            var customParamFields = schema.properties["list.click.customparams"].replace(" ", "").split(",");
            for (var param in customParamFields) {
                if (!customParamFields.hasOwnProperty(param)) {
                    continue;
                }
                customParams[param] = {};
                customParams[param]["key"] = customParamFields[param];
                customParams[param]["value"] = rowdm.fields[customParamFields[param]];
            }
        }
        return customParams;
    }


    //#region listeners
    $scope.$on("sw.crud.applicationchanged", function (event, schema, datamap, panelid) {
        if ($scope.panelid === panelid) {
            //need to re fetch the selection model since the context whenever the application changes
            $scope.selectionModel = crudContextHolderService.getSelectionModel($scope.panelid);
        }
    });

    //#endregion listeners

}

window.BaseList = BaseList;

})(angular);