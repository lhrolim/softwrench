﻿softwrench.controller('CrudCompositionDetailController',
    function ($log, $scope, $rootScope, crudContextService, fieldService, offlineCompositionService, offlineAssociationService) {

        'use strict';

        function init() {
            $scope.schema = crudContextService.getCompositionDetailSchema();
            $scope.displayables = $scope.schema.displayables;
            $scope.datamap = crudContextService.getCompositionDetailItem();
            $scope.allowsUpdate = offlineCompositionService.allowsUpdate(crudContextService.getCompositionDetailItem(), crudContextService.getCompositionListSchema());
        }

        $scope.fieldLabel = function (item,field) {
            return field.label + ":" + item[field.attribute];
        }

        $scope.associationSearch = function (queryparameters) {
            return offlineAssociationService.filterPromise(crudContextService.currentDetailSchema(), $scope.datamap, queryparameters.identifier, queryparameters.query);
        }

        $scope.getAssociationLabelField = function (fieldMetadata) {
            return 'datamap.' + fieldMetadata.labelFields[0];
        }

        $scope.getAssociationValueField = function (fieldMetadata) {
            return 'datamap.' + fieldMetadata.valueField;
        }

        $scope.visibleFields = function () {
            var schema = crudContextService.compositionListSchema();
            return fieldService.getVisibleDisplayables({}, schema);
        }
        $scope.isFieldHidden = function (fieldMetadata) {
            return fieldService.isFieldHidden($scope.datamap, $scope.schema, fieldMetadata);
        }

        $rootScope.$on('$stateChangeSuccess',
        function (event, toState, toParams, fromState, fromParams) {
            if (toState.name.startsWith("main.cruddetail.compositiondetail")) {
                //needs to refresh the displayables and datamap everytime the detail page is loaded.
                init();
            }
        });

        init();

    }
);