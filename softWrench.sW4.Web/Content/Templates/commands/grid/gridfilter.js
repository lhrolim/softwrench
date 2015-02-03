var app = angular.module('sw_layout');

app.controller('GridFilterController', ['$scope', '$http', 'userPreferenceService', 'searchService', 'i18NService', 'alertService', 'contextService',
        function($scope, $http, userPreferenceService, searchService, i18NService, alertService, contextService) {

            function init() {
                //since this is inside a ng-include this controller is getting reevaluated every time we refresh the page
                $scope.selectedfilter = contextService.getFromContext("selectedfilter", true, true);
                var basicMode = contextService.getFromContext("filter_basicmode", true, true);
                if (basicMode == undefined) {
                    basicMode = true;
                }
                $scope.basicMode = basicMode;
            }

            init();


            $scope.nonSharedFilters = function() {
                return userPreferenceService.loadUserNonSharedFilters($scope.schema.applicationName, $scope.schema.schemaId);
            }

            $scope.sharedFilters = function() {
                return userPreferenceService.loadUserSharedFilters($scope.schema.applicationName, $scope.schema.schemaId);
            }

            $scope.hasSharedFilter = function() {
                return $scope.sharedFilters().length > 0;
            }

            $scope.hasFilter = function() {
                return userPreferenceService.hasFilter($scope.schema.applicationName, $scope.schema.schemaId);
            }

            $scope.toggleFilterMode = function() {
                $scope.basicMode = !$scope.basicMode;
                for (var data in $scope.searchData) {
                    $scope.searchData[data] = "";
                }
                // Clear the previous filterFixedWhereClause when switching filter modes
                $scope.paginationData.filterFixedWhereClause = null;
                contextService.insertIntoContext("filter_basicmode", $scope.basicMode, true);
                searchService.toggleAdvancedFilterMode();
            }

            $scope.i18N = function(key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.refreshGrid = function() {
                contextService.insertIntoContext("filter_basicmode", $scope.basicMode, true);
                searchService.refreshGrid();
            };

            $scope.saveFilter = function() {
                if (!$scope.selectedfilter && !$scope.hasFilterData()) {
                    alertService.alert('Please, fill any grid filter in order to save it');
                    return;
                }

                var saveFormSt = $("#savefilterform").prop('outerHTML');
                //TODO: use angularjs?!
                //remove display:none
                saveFormSt = saveFormSt.replace('none', '');
                //change id of the filter so that it becomes reacheable via jquery
                saveFormSt = saveFormSt.replace('savefiltername', 'savefiltername2');
                bootbox.dialog({
                    message: saveFormSt,
                    title: "Save Filter",
                    buttons: {
                        cancel: {
                            label: $scope.i18N('.cancel', 'Cancel'),
                            className: "btn btn-default",
                            callback: function() {
                                return null;
                            }
                        },
                        main: {
                            label: $scope.i18N('_grid.filter.savefiltebtn', 'Save'),
                            className: "btn-primary",
                            callback: function(result) {
                                if (result) {
                                    $scope.createFilter($('#savefiltername2').val());
                                }
                            }
                        }
                    },
                    className: "smallmodal"
                });
            }

            $scope.hasFilterData = function() {
                var searchData = $scope.searchData;
                for (var data in searchData) {
                    if (data == "lastSearchedValues") {
                        continue;
                    }
                    return searchData[data] != "";
                }
                return false;
            }

            $scope.deleteFilter = function() {
                var filter = $scope.selectedfilter;
                alertService.confirm(null, null, function() {
                    userPreferenceService.deleteFilter(filter.id, filter.creatorId, function() {
                        $scope.selectedfilter = null;
                    });
                }, "Are you sure that you want to remove filter {0}?".format(filter.alias), null);
            }

            $scope.createFilter = function(alias) {
                var id = $scope.selectedfilter ? $scope.selectedfilter.id : null;
                var owner = $scope.selectedfilter ? $scope.selectedfilter.creatorId : null;
                userPreferenceService.saveFilter($scope.schema, $scope.searchData, $scope.searchOperator, alias, id, owner,
                    function(filter) {
                        $scope.selectedfilter = filter;
                    });


            }

            $scope.applyFilter = function(filter) {
                var fieldsArray = filter.fields.split(',');
                var operatorsArray = filter.operators.split(',');
                var valuesArray = filter.values.split(',,,');

                for (var i = 0; i < fieldsArray.length; i++) {
                    var field = fieldsArray[i];
                    $scope.searchData[field] = valuesArray[i];
                    $scope.searchOperator[field] = searchService.getSearchOperationBySymbol(operatorsArray[i]);
                }
                $scope.selectedfilter = filter;
                //this is required because the controller is reinitialized on an, until now unpredictable way
                contextService.insertIntoContext("selectedfilter", filter, true);
                contextService.insertIntoContext("filter_basicmode", $scope.basicMode, true);
                searchService.refreshGrid();
            }

            $scope.clearFilter = function() {
                contextService.insertIntoContext("selectedfilter", null, true);
                $scope.selectedfilter = null;
                $scope.searchOperator = {};
                searchService.refreshGrid({});
            }

            $scope.$on("sw_redirectapplicationsuccess", function(event) {
                contextService.insertIntoContext("selectedfilter", null, true);
                $scope.selectedfilter = null;
            });

        }
    ]);