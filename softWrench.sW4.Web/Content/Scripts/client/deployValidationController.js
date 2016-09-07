(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("DeployValidationController", DeployValidationController);
    DeployValidationController.$inject = ["$scope", "$http", "restService", "i18NService", "alertService"];
    function DeployValidationController($scope, $http, restService, i18NService, alertService) {
        "ngInject";

        $scope.failureTooltip = "Failure - Click here for more details";
        $scope.testSuccessCount = 0;
        $scope.testFailureCount = 0;
        $scope.missingDataCount = 0;
        $scope.showOnlyFailure = true;
        $scope.excludeMissingTestData = true;

        $scope.validate = function () {
            $scope.testSuccessCount = 0;
            $scope.testFailureCount = 0;

            restService.getPromise("DeployValidation", "Validate", {}).then(response => {
                if (!response.data || !response.data.resultObject) {
                    return;
                }
                var applicationTests = response.data.resultObject;

                angular.forEach(applicationTests, (testResult, key) => {
                    if (!applicationTests.hasOwnProperty(key)) {
                        return;
                    }
                    var application = $scope.applications[key];
                    if (!application) {
                        return;
                    }

                    application.validationResultList = testResult.validationResultList;

                    angular.forEach(application.validationResultList, (valResult, valkey) => {
                        if (valResult.missingTestData) {
                            $scope.missingDataCount += 1;
                            application.missingTestData = valResult.missingTestData;
                            
                        } else {
                            if (valResult.hasProblems) {
                                $scope.testFailureCount += 1;
                                application.hasfailedTests = valResult.hasProblems;

                            } else {
                                $scope.testSuccessCount += 1;
                            }
                        }
                    });
                });

            });
        }

        $scope.showFailures = function (application, validation) {
            if (!validation || validation.hasProblems !== true) {
                return;
            }
            
            const data = {};

            var msg = `For the opertion ${validation.actionName} (${validation.actionDescription}) for the application "${application.title}":`;
            if (validation.missingWsdl) {
                msg += "<br />";
                msg += "- The WSDL is missing.";
            }
            if (validation.missingProperties && validation.missingProperties.length > 0) {
                msg += "<br />";
                msg += "- Missing Properties: ";
                msg += validation.missingProperties.join(", ");
                msg += ".";
            }
            if (validation.exClassName) {
                msg += "<br />";
                msg += "- An exception was thrown!!!";

                data.exceptionType = validation.exClassName;
                data.outlineInformation = validation.exMsg;
                data.stackTrace = validation.exStack;
            }

            data.errorMessage = msg;
            alertService.notifyexception(data);
        }

        $scope.i18N = function (key, defaultValue, paramArray) {
            return i18NService.get18nValue(key, defaultValue, paramArray);
        };

        $scope.applications = $scope.resultData;

        const checkState = function(application, isUpdate, failureState) {
            if (isUpdate) {
                return application.updateValidation && application.updateValidation.hasProblems === failureState;
            }
            return application.hasCreationSchema && application.createValidation && application.createValidation.hasProblems === failureState;
        };

        $scope.hasProblemns = function (application, isUpdate) {
            return checkState(application, isUpdate, true);
        }

        $scope.succeded = function (application, isUpdate) {
            return checkState(application, isUpdate, false);
        }

        $scope.isMissingTestData = function (application, isUpdate) {
            if (isUpdate) {
                return application.updateValidation && application.updateValidation.missingTestData;
            }
            return application.hasCreationSchema && application.createValidation && application.createValidation.missingTestData;
        }
    };
})(angular);