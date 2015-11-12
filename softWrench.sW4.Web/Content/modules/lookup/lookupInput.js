(function (angular) {
    "use strict";

    angular.module("sw_lookup").directive("lookupInput", ["contextService", function (contextService) {

        var directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/modules/lookup/templates/lookupinput.html"),
            $scope: {
                datamap: "=",
                parentdata:"=",
                schema: "=",
                fieldMetadata: "=",
                disabledassociations: "=",
                blockedassociations: "=",
                lookupAssociationsCode: "=",
                associationOptions:"=",
                lookupAssociationsDescription: "=",
                mode:"@"
            },

            link: function (scope, element, attrs) {
                scope.config.element = element;
            },

            controller: ["$scope", "cmplookup", "expressionService", "cmpfacade", "dispatcherService", "modalService", "compositionCommons", function ($scope, cmplookup, expressionService, cmpfacade, dispatcherService, modalService, compositionCommons) {
                $scope.config = { element: null };

                $scope.lookupObj = {};

                $scope.shouldShowDescription = function (fieldMetadata) {
                    return "composition" !== $scope.mode && fieldMetadata.hideDescription !== true;
                }

                $scope.isSelectEnabled = function (fieldMetadata, datamap) {
                    var searchDatamap = datamap;
                    if ($scope.parentdata) {
                        searchDatamap = compositionCommons.buildMergedDatamap(datamap, $scope.parentdata);
                    }

                    var key = fieldMetadata.associationKey;
                    $scope.disabledassociations = $scope.disabledassociations || {};
                    if (!key) {
                        return true;
                    }
                    var result = ($scope.blockedassociations == null || !$scope.blockedassociations[key]) && expressionService.evaluate(fieldMetadata.enableExpression, searchDatamap, $scope);
                    if (result !== $scope.disabledassociations[key]) {
                        cmpfacade.blockOrUnblockAssociations($scope, !result, !$scope.disabledassociations[key], fieldMetadata);
                        $scope.disabledassociations[key] = result;
                    }
                    return result;
                };

                $scope.lookupCodeChange = function (fieldMetadata) {
                    var allowFreeText = fieldMetadata.rendererParameters["allowFreeText"];
                    if (allowFreeText === "true") {
                        var code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                        $scope.datamap[fieldMetadata.target] = code;
                    }
                };

                $scope.lookupCodeBlur = function (fieldMetadata) {
                    var code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                    var targetValue = $scope.datamap[fieldMetadata.target];
                    var allowFreeText = fieldMetadata.rendererParameters["allowFreeText"];

                    if (code !== targetValue) {
                        if (!code) {
                            $scope.datamap[fieldMetadata.target] = null;
                        } else if (allowFreeText !== "true") {
                            $scope.showLookupModal(fieldMetadata);
                        }
                    }
                };

                $scope.showCustomModal = function (fieldMetadata, schema, datamap) {
                    if (fieldMetadata.rendererParameters["schema"] != undefined) {
                        var service = fieldMetadata.rendererParameters["onsave"];
                        var savefn = function () { };
                        if (service != null) {
                            var servicepart = service.split(".");
                            savefn = dispatcherService.loadService(servicepart[0], servicepart[1]);
                        }

                        var modaldatamap = null;

                        var onloadservice = fieldMetadata.rendererParameters["onload"];
                        if (onloadservice != null) {
                            var onloadservicepart = onloadservice.split(".");
                            var onloadfn = dispatcherService.loadService(onloadservicepart[0], onloadservicepart[1]);
                            modaldatamap = onloadfn(datamap, fieldMetadata.rendererParameters["schema"], fieldMetadata);
                        }

                        modalService.show(fieldMetadata.rendererParameters["schema"], modaldatamap, {}, function (selecteditem) {
                            savefn(datamap, fieldMetadata.rendererParameters["schema"], selecteditem, fieldMetadata);
                        }, null, datamap, schema);

                        return;
                    }
                };

                $scope.showLookupModal = function (fieldMetadata) {
                    $scope.lookupAssociationsDescription = $scope.lookupAssociationsDescription || {};

                    if (fieldMetadata.rendererType === "modal") {
                        this.showCustomModal(fieldMetadata, $scope.schema, $scope.datamap);
                        return;
                    }

                    var searchDatamap = $scope.datamap;

                    if ($scope.parentdata) {
                        $scope.lookupObj.parentdata = $scope.parentdata;
                        $scope.lookupObj.item = $scope.datamap;
                        searchDatamap = compositionCommons.buildMergedDatamap($scope.datamap, $scope.parentdata);
                    }

                    if (!$scope.isSelectEnabled(fieldMetadata, searchDatamap)) {
                        return;
                    }

                    var code = "";
                    if ($scope.lookupAssociationsCode[fieldMetadata.attribute] !== $scope.datamap[fieldMetadata.attribute]) {
                        code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                    }

                    $scope.lookupObj.element = $scope.config.element;
                    cmplookup.updateLookupObject($scope, fieldMetadata, code, searchDatamap);
                };

            }]

        };

        return directive;

    }]);

})(angular);