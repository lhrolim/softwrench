(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("DeployValidationController", DeployValidationController);
    function DeployValidationController($scope, $http, restService, i18NService, alertService) {
        "ngInject";

        $scope.failureTooltip = "Failure - Click here for more details";

        $scope.validate = function () {
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
                    application.createValidation = testResult.createValidation;
                    application.updateValidation = testResult.updateValidation;
                });
            });
        }

        $scope.showFailures = function (application, isUpdate) {
            const validation = isUpdate ? application.updateValidation : application.createValidation;
            if (!validation || validation.hasProblems !== true) {
                return;
            }
            
            const data = {};

            var operation = isUpdate ? "update" : "creation";
            var msg = `For the ${operation} of the application "${application.title}":`;
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