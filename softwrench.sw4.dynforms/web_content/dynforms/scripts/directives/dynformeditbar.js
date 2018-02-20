(function (angular) {
    'use strict';


    function dynFormDetailEditBarController($scope, crudContextHolderService, schemaService, modalService, schemaCacheService, fieldService, alertService, dynFormService) {


        $scope.isPreviewMode = function() {
            return dynFormService.isPreviewMode();
        }

        $scope.isEditAllowed = function () {
            const schema = crudContextHolderService.currentSchema();
            //TODO: create wrapper fn
            return !schemaService.isPropertyTrue(schema, "dynforms.editionallowed");
        }

        $scope.shouldShowIcon = function (type) {
           return true;
        }

        $scope.remove = function () {
            return dynFormService.removeDisplayable($scope.fieldMetadata).then(r => {
                return $scope.$emit(JavascriptEventConstants.ReevalDisplayables);
            });
        }

        $scope.shouldShowBlankCommand = function () {
            const cs = crudContextHolderService.currentSchema();
            const numItems = cs.displayables.length;
            //one goes for the section itself and another for the hidden id
            return numItems === 2;
        }

        $scope.addField = function (direction) {
            return dynFormService.addDisplayable($scope.fieldMetadata, direction).then(r => {
                return $scope.$emit(JavascriptEventConstants.ReevalDisplayables);
            });
        }

        $scope.edit = function () {
            return dynFormService.editDisplayable($scope.fieldMetadata).then(r => {
                return $scope.$emit(JavascriptEventConstants.ReevalDisplayables);
            });
        }

        $scope.isEditingSection = function() {
            return dynFormService.isEditingSection();
        }

        $scope.toggleSectionSelection = function() {
            dynFormService.toggleSectionSelection($scope.fieldMetadata);
        }

        $scope.fieldMoved = function (fieldMetadata) {
//            console.log("ok");
        }


    }


    dynFormDetailEditBarController.$inject = ['$scope', "crudContextHolderService", "schemaService", "modalService", "schemaCacheService", "fieldService", "alertService","dynFormService"];

    angular.module("dynforms").controller('dynformDetailEditBarSharedCtrl', dynFormDetailEditBarController);

    angular.module("dynforms")
        .directive("dynformDetailEditBar", ["contextService", function (contextService) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: contextService.getResourceUrl('/Content/Shared/dynforms/htmls/dynformdetaileditbar.html'),
                scope: {
                    fieldMetadata: '=',
                    extraparameters: '='
                },

                controller: "dynformDetailEditBarSharedCtrl"
            };
        }]);


    angular.module("dynforms")
        .directive("dynformDetailEditBarWrapper", ["$compile", "crudContextHolderService","schemaService", function ($compile, crudContextHolderService, schemaService) {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    fieldmetadata: '=',
                    extraparameters: '=',
                    schema: '=',
                    mode: '@'
                },

                link: function (scope, element, attrs) {

                    scope.$name = 'dynformDetailEditBar';

                    var doLoad = function () {
                        const isSection = scope.fieldmetadata.type === "ApplicationSection";

                        if ((isSection && scope.mode === "section") || !isSection && scope.mode === "label") {
                            element.append("<dynform-detail-edit-bar field-metadata='fieldmetadata' extraparameters='extraparameters'/>");    
                        }

                        $compile(element.contents())(scope);
                    }
//                    const extraparameters = scope.extraparameters || scope.fieldmetadata.extraparameters || [];
                    
                    const isEdition = scope.schema.properties["dynforms.editionallowed"] === "true";

                    if (isEdition) {
                        //inline compositions should load automatically
                        doLoad();

                    }
                }
            }
        }]);






})(angular);