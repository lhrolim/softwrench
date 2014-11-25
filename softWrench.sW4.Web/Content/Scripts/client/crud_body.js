﻿var app = angular.module('sw_layout');


app.directive('tabsrendered', function ($timeout, $log, $rootScope) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('.compositiondetailtab li>a').each(function () {
                        var $this = $(this);
                        $this.click(function (e) {
                            e.preventDefault();
                            $this.tab('show');
                            var tabId = $(this).data('tabid');
                            $log.getInstance('tabsrendered').trace('lazy loading tab {0}'.format(tabId));
                            $rootScope.$broadcast('sw_lazyloadtab', tabId);
                        });
                    });
                });
            }
        }
    };
});


app.directive('crudBody', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_body.html'),
        scope: {
            isList: '=',
            isDetail: '=',
            blockedassociations: '=',
            associationOptions: '=',
            associationSchemas: '=',
            schema: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            paginationdata: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            checked: '='
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            validationService, submitService, redirectService,
            associationService, contextService) {

            $scope.$name = 'crudbody' + ($scope.ismodal  == "false"? 'modal' : '');

        

          

            $scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return null;
                }
                return formattedValue;
            };

            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isCommand = function (schema) {
                if ($scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };
            $scope.isNotHapagTest = function () {
                if ($rootScope.clientName != 'hapag')
                    return true;
            }
            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                if (!$rootScope.showSuccessMessage) {
                    fixHeaderService.resetTableConfig($scope.schema);
                }
            });


            $scope.$on('sw_errormessage', function (event, show) {
                fixHeaderService.topErrorMessageHandler(show, $scope.$parent.isDetail, $scope.schema);
            });

            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {
                //make sure we are seeing the top of the grid 
                window.scrollTo(0, 0);
            });

            function defaultSuccessFunction(data) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                if (data != null) {
                    if (data.type == 'ActionRedirectResponse') {
                        //we´ll not do a crud action on this case, so totally different workflow needed
                        redirectService.redirectToAction(null, data.controller, data.action, data.parameters);
                    } else {
                        var nextSchema = data.schema;
                        $scope.$parent.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
                    }
                }
            }


            $scope.isEditing = function (schema) {
                var idFieldName = schema.idFieldName;
                var id = $scope.datamap.fields[idFieldName];
                return id != null;
            };


            $scope.shouldShowField = function (expression) {
                if (expression == "true") {
                    return true;
                }
                var stringExpression = '$scope.datamap.' + expression;
                var ret = eval(stringExpression);
                return ret;
            };


            $scope.isHapag = function () {
                return $rootScope.clientName == "hapag";
            };



            $scope.renderListView = function (parameters) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                var listSchema = 'list';
                if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                    //if we have a list schema already declared, keep it
                    listSchema = $scope.schema.schemaId;
                }
                $scope.$parent.renderView($scope.$parent.applicationname, listSchema, 'none', $scope.title, parameters);
            };

            $scope.toListSchema = function (data, schema) {
                /*if (schema instanceof Array) {
                    $scope.$parent.multipleSchemaHandling($scope.$parent.resultObject);
                } else {*/
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                $('#crudmodal').modal('hide');
                $scope.showingModal = false;
                if (GetPopUpMode() == 'browser') {
                    open(location, '_self').close();
                }
                $scope.schema = schema;
                $scope.datamap = data;
                if ($scope.schema == null || $scope.datamap == null || $scope.schema.stereotype == 'Detail') {
                    $scope.renderListView(null);
                    return;
                } else {
                    $scope.$emit('sw_titlechanged', schema.title);
                    $scope.$parent.toList(null);
                }
                //}
            };

            $scope.delete = function () {

                var schema = $scope.schema;
                var idFieldName = schema.idFieldName;
                var applicationName = schema.applicationName;
                var id = $scope.datamap.fields[idFieldName];

                var parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    parameters.mockmaximo = true;
                }
                parameters.platform = platform();
                parameters = addSchemaDataToParameters(parameters, $scope.schema);
                var deleteParams = $.param(parameters);

                var deleteURL = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                $http.delete(deleteURL)
                    .success(function (data) {
                        defaultSuccessFunction(data);
                    });

            };

            $scope.save = function (selecteditem, parameters) {
                if (parameters == undefined) {
                    parameters = {};
                }
                var successCbk = parameters.successCbk;
                var failureCbk = parameters.failureCbk;
                var nextSchemaObj = parameters.nextSchemaObj;
                var applyDefaultSuccess = parameters.applyDefaultSuccess;
                var applyDefaultFailure = parameters.applyDefaultFailure;
                var isComposition = parameters.isComposition;

                //selectedItem would be passed in the case of a composition with autocommit=true. 
                //Otherwise, fetching from the $scope.datamap
                var fromDatamap = selecteditem == null;
                var itemToSave = fromDatamap ? $scope.datamap : selecteditem;
                var fields = fromDatamap ? itemToSave.fields : itemToSave;
                var applicationName = $scope.schema.applicationName;
                var idFieldName = $scope.schema.idFieldName;
                var id = fields[idFieldName];
                if (sessionStorage.mockclientvalidation == undefined) {
                    var validationErrors = validationService.validate($scope.schema.displayables, fields);
                    if (validationErrors.length > 0) {
                        //interrupting here, can´t be done inside service
                        return;
                    }
                }
                //some fields might require special handling
                submitService.removeNullInvisibleFields($scope.schema.displayables, fields);
                fields = submitService.removeExtraFields(fields, true, $scope.schema);
                submitService.translateFields($scope.schema.displayables, fields);
                associationService.insertAssocationLabelsIfNeeded($scope.schema, fields, $scope.associationOptions);

                //hook for updating doing custom logic before sending the data to the server
                $rootScope.$broadcast("sw_beforeSave", fields);


                var jsonString = angular.toJson(fields);

                parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    //this will cause the maximo layer to be mocked, allowing testing of workflows without actually calling the backend
                    parameters.mockmaximo = true;
                }
                parameters = addSchemaDataToParameters(parameters, $scope.schema, nextSchemaObj);
                parameters.platform = platform();

                $rootScope.savingMain = !isComposition;

                if (isIe9()) {
                    var formToSubmitId = submitService.getFormToSubmitIfHasAttachement($scope.schema.displayables, fields);
                    if (formToSubmitId != null) {
                        var form = $(formToSubmitId);
                        submitService.submitForm(form, parameters, jsonString, applicationName);
                        return;
                    }
                }

                var saveParams = $.param(parameters);
                var urlToUse = url("/api/data/" + applicationName + "/" + id + "?" + saveParams);
                var command = id == null ? $http.post : $http.put;

                command(urlToUse, jsonString)
                    .success(function (data) {
                        if (successCbk == null || applyDefaultSuccess) {
                            defaultSuccessFunction(data);
                        }
                        if (successCbk != null) {
                            successCbk(data);
                        }
                    })
                    .error(function (data) {
                        if (failureCbk != null) {
                            failureCbk(data);
                        }
                    });
            };



            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

        }
    };
});

