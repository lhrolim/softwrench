(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("BaseList", BaseList);

    //idea took from  https://www.exratione.com/2013/10/two-approaches-to-angularjs-controller-inheritance/
    BaseList.$inject = ["$scope", "$log", "formatService", "expressionService", "searchService", "fieldService", "i18NService", "commandService", "crudContextHolderService", "gridSelectionService", "dispatcherService", "controllerInheritanceService"];
    function BaseList($scope, $log, formatService, expressionService, searchService, fieldService, i18NService, commandService, crudContextHolderService, gridSelectionService, dispatcherService, controllerInheritanceService) {

        $scope.getFormattedValue = function (value, column, datamap) {
            var formattedValue = formatService.format(value, column, datamap);
            if (formattedValue === "-666") {
                //this magic number should never be displayed! 
                //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                return null;
            }
            return formattedValue;
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
                return operation.id.equalsAny("CONTAINS", "EQ", "NCONTAINS", "STARTWITH", "GT", "LT", "ENDWITH", "BLANK");
            } else if (filter.type === "MetadataNumberFilter") {
                return operation.id.equalsAny("GT", "LT", "GTE", "LTE", "EQ", "NOTEQ", "BLANK");
            } else if (filter.type === "MetadataDateTimeFilter") {
                return operation.id.equalsAny("GT", "LT", "GTE", "LTE", "BTW");
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
            if ($scope.filter.type === "MetadataDateTimeFilter") {
                return searchService.getSearchOperationById("GTE");
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

        $scope.shouldShowHeaderLabel = function (column) {
            //console.log(column.rendererParameters.headericon);

            if (column.rendererParameters.headericon != undefined) {
                return column.rendererParameters.headericon;
            }

            return (column.type === "ApplicationFieldDefinition" || column.type === "OptionField") && column.rendererType !== "color" && column.rendererType !== "icon" && column.rendererType !== "iconbutton";
        };

        $scope.headerIcon = function (column) {
            if (column.rendererParameters.headericon == undefined) {
                return '';
            }

            if (column.rendererParameters.headericon) {
                return column.rendererParameters.icon;
            }
        };

        $scope.hasIcon = function (column) {
            if (column.rendererParameters.icon == undefined) {
                return false;
            }

            return column.rendererParameters.icon != undefined;
        };

        $scope.shouldShowHeaderFilter = function (column) {
            return $scope.shouldShowHeaderLabel(column) && !column.rendererParameters["hidefilter"];
        };

        $scope.changePriority = function (rowdm, attribute, newPriority) {
            var column = fieldService.getDisplayableByKey($scope.schema, attribute);
            var clickService = column.rendererParameters.onclick;
            var newValue = {
                attribute: attribute,
                value: newPriority
            }

            commandService.executeClickCustomCommand(clickService, rowdm.fields, column, $scope.schema, $scope.panelid, newValue);
        };

        $scope.showDetail = function (rowdm, attribute, forceEdition) {

            var mode = $scope.schema.properties['list.click.mode'];
            var popupmode = $scope.schema.properties['list.click.popupmode'];
            var schemaid = $scope.schema.properties['list.click.schema'];
            var fullServiceName = $scope.schema.properties['list.click.service'];
            var editDisabled = $scope.schema.properties['list.disabledetails'];
            var commandResult = null;

            var column = fieldService.getDisplayableByKey($scope.schema, attribute);
            var selectionModel = crudContextHolderService.getSelectionModel($scope.panelid);
            var autoloadcomposition = column.rendererParameters.autoloadcomposition;

            //force edition means that the user has clicked the edition icon, so regardless of the mode we need to open the details
            if (selectionModel.selectionMode && !forceEdition) {
                commandResult = null;
                if (fullServiceName != null) {
                    commandResult = commandService.executeClickCustomCommand(fullServiceName, rowdm.fields, column, $scope.schema, $scope.panelid);
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
            // TODO: Result from custom list click MUST be false to stop execution of default list click?
            if (fullServiceName != null) {
                commandResult = commandService.executeClickCustomCommand(fullServiceName, rowdm.fields, column, $scope.schema, $scope.panelid);
                if (commandResult === false) {
                    return;
                }else if (commandResult.then) {
                    return commandResult.then(function() {
                        $scope.doShowDetail(rowdm, schemaid, mode, popupmode);
                    });
                }
            };

            $scope.doShowDetail(rowdm, schemaid, mode, popupmode, autoloadcomposition);

        };

        $scope.doShowDetail = function (rowdm, schemaid, mode, popupmode, loadcompositiontab) {
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
                id: id, popupmode: popupmode, customParameters: $scope.getCustomParameters($scope.schema, rowdm), autoloadcomposition: loadcompositiontab
            });
        }


        $scope.getCustomParameters = function (schema, rowdm) {
            var customParams = {};
            if (schema.properties["list.click.customparams"]) {
                var customParamFields = schema.properties["list.click.customparams"].replace(" ", "").split(",");
                for (var param in customParamFields) {
                    if (!customParamFields.hasOwnProperty(param)) {
                        continue;
                    }

                    var fieldName = customParamFields[param];
                    var fieldValue = rowdm.fields[fieldName];
                    // fieldValue not found: maybe a transientField --> find by matching attributeToServer 
                    if (!fieldValue) {
                        var actualField = schema.displayables.find(function(field) {
                            return field.attributeToServer === fieldName;
                        });
                        if (!!actualField) {
                            fieldValue = rowdm.fields[actualField.attribute];
                        }
                    }

                    customParams[param] = {};
                    customParams[param]["key"] = fieldName;
                    customParams[param]["value"] = fieldValue;
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

        //$scope, $log, formatService, expressionService, searchService, fieldService, i18NService, commandService, crudContextHolderService, gridSelectionService, dispatcherService, controllerInheritanceService

        controllerInheritanceService
            .begin(this)
            .inherit(BaseController, {
                $scope: $scope,
                $log: $log,
                i18NService: i18NService,
                fieldService: fieldService,
                formatService: formatService,
                crudContextHolderService: crudContextHolderService,
                expressionService: expressionService
            });

    }




    window.BaseList = BaseList;

})(angular);