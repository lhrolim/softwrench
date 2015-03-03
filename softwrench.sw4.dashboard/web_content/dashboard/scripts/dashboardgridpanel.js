var app = angular.module('sw_layout');

app.directive('dashboardgridpanel', function ($timeout, $log, $rootScope, contextService, redirectService, searchService,
                                                validationService, associationService, fixHeaderService, dashboardAuxService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboardgridpanel.html'),
        scope: {
            panelrow: '=',
            panelcol: '@',
            paneldatasource: '=',
            dashboardid:'='
        },

        controller: function ($scope, $http, $rootScope) {

            $scope.toList = function (data, scope) {
                if (scope == null) {
                    scope = $scope;
                }

                $scope.$broadcast("sw_gridrefreshed", data, $rootScope.printRequested);

                if (data != null) {
                    //TODO: rethink about it
                    $scope.paginationData = {};
                    $scope.searchValues = data.searchValues;
                    $scope.paginationData.pagesToShow = data.pagesToShow;
                    $scope.paginationData.pageNumber = data.pageNumber;
                    $scope.paginationData.selectedPage = data.pageNumber;
                    $scope.paginationData.pageCount = data.pageCount;
                    $scope.paginationData.pageSize = data.pageSize;
                    $scope.paginationData.paginationOptions = data.paginationOptions;
                    $scope.paginationData.totalCount = data.totalCount;
                    $scope.paginationData.hasPrevious = data.hasPrevious;
                    $scope.paginationData.hasNext = data.hasNext;
                    $scope.paginationData.filterFixedWhereClause = data.filterFixedWhereClause;
                    $scope.searchData = {};
                    $scope.searchOperator = {};

                    if (data.pageResultDto && data.pageResultDto.searchParams) {
                        var result = searchService.buildSearchDataAndOperations(data.pageResultDto.searchParams, data.pageResultDto.searchValues);
                        $scope.searchData = result.searchData;
                        $scope.searchOperator = result.searchOperator;
                    }
                }

                switchMode(false, $scope);
            };

            $scope.renderView = function (applicationName, schemaId, mode, title, parameters) {
                if (parameters === undefined || parameters == null) {
                    parameters = {};
                }

                if (title == null) {
                    title = $scope.title;
                }

                parameters.key = {};
                parameters.key.schemaId = schemaId;
                parameters.key.mode = mode;
                parameters.key.platform = platform();
                parameters.customParameters = {};

                parameters.title = title;
                parameters.printMode = null;

                $scope.applicationname = applicationName;
                $scope.requestmode = mode;

                var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));

                $http.get(urlToCall)
                    .success(function (data) {
                        $scope.renderData(data);
                    })
                    .error(
                        function (data) {
                            var errordata = {
                                errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                                errorStack: data.message
                            }

                            $rootScope.$broadcast("sw_ajaxerror", errordata);
                        });
            };

            $scope.renderData = function renderData(result) {
                contextService.insertIntoContext("associationsresolved", false, true);
                $scope.isList = true;
                $scope.crudsubtemplate = null;
                $scope.multipleSchema = false;
                $scope.schemas = null;

                // Update displayable with applied application filter 
                if ($scope.appFields != null) {
                    var index;
                    var appFields = $scope.appFields.split(',');

                    for (index = 0; index < result.schema.displayables.length; index++) {
                        var displayField = result.schema.displayables[index];

                        if (appFields.indexOf(displayField.attribute) < 0) {
                            result.schema.displayables[index].isHidden = true;
                        }
                    }
                }

                $scope.previousschema = $scope.schema;
                $scope.previousdata = $scope.datamap;

                $scope.schema = result.schema;

                // resultObject can be null only when SW is pointing to a Maximo DB different from Maximo WS DB
                $scope.datamap = instantiateIfUndefined(result.resultObject);

                $scope.extraparameters = instantiateIfUndefined(result.extraParameters);

                $scope.mode = result.mode;
                if ($scope.schema != null) {
                    $scope.schema.mode = $scope.mode;
                }

                validationService.clearDirty();

                // It should be application list result; however, we should do a validation to make sure
                if (result.type == 'ApplicationListResult') {
                    $scope.toList(result, $scope);

                    associationService.updateAssociationOptionsRetrievedFromServer($scope, result.associationOptions, null);
                    fixHeaderService.FixHeader();

                    $scope.$broadcast('sw_griddatachanged', $scope.datamap, $scope.schema);
                }

                $scope.isDataAvailable = true;
            };

            function switchMode(mode, scope) {
                if (scope == null) {
                    scope = $scope;
                }

                scope.isDetail = mode;
                scope.isList = !mode;

                var crud_context;

                if (scope.isList) {
                    var elements = [];
                    for (var i = 0; i < $scope.datamap.length; i++) {

                        elements.push($scope.datamap[i].fields[$scope.schema.idFieldName]);
                    }
                    crud_context = {
                        list_elements: elements,
                        detail_next: "0",
                        detail_previous: "-1"
                    };

                    contextService.insertIntoContext("crud_context", crud_context);
                }
            }

            $scope.getPanelSourceData = function () {
                var dashboardPanelInfo = $scope.paneldatasource;

                if (dashboardPanelInfo != null) {

                    $scope.title = dashboardPanelInfo.panel['title'];
                    $scope.dashboardpanelid = dashboardPanelInfo.id;
                    $scope.pageSize = dashboardPanelInfo.panel['limit'];
                    $scope.defaultSortField = dashboardPanelInfo.panel["defaultSortField"];
                    $scope.appFields = dashboardPanelInfo.panel["appFields"];
                    $scope.searchSort = {};

                    var schemaReference = dashboardPanelInfo.panel['schemaRef'];
                    var applicationName = dashboardPanelInfo.panel['application'];

                    var parameters = {};
                    parameters.printMode = null;
                    parameters.SearchDTO = {
                        SearchAscending: true,
                        filterFixedWhereClause: null,
                        needsCountUpdate: true,
                        pageNumber: 1,
                        pageSize: $scope.pageSize,
                        paginationOptions: [10, 30, 100],
                        searchParams: "",
                        searchSort: $scope.defaultSortField,
                        searchValues: ""
                    };

                    $scope.renderView(applicationName, schemaReference, null, $scope.title, parameters);
                }
            }


            $scope.isDataAvailable = false;

            $scope.getPanelSourceData();


            $scope.$on('sw_lazyloaddashboard', function (event, tabId) {
                if (tabId == $scope.dashboardid) {
                    $scope.getPanelSourceData();
                }
            });

            $scope.$on('sw_renderview', function (event, applicationName, schemaId, mode, title, parameters, dashboardpanelid) {
                if (dashboardpanelid != null) {
                    $scope.renderView(applicationName, schemaId, mode, title, parameters);
                }
            });
        },

        link: function (scope, element, attrs) {

        }
    }
});